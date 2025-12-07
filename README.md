# Desafio – Backend de Ativação de DIDs

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Dev-239120?logo=csharp&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-Logic_Apps-0078D4?logo=microsoft-azure&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Hexagonal-4B32C3)
![Polly](https://img.shields.io/badge/Resilience-Polly-7B42F6)

Este repositório contém a implementação de um backend de ativação de DIDs (números virtuais de telefone), pensado para ser plugado em um fluxo de **Azure Logic Apps** conforme o desafio proposto.

O foco foi:

- Garantir um **contrato interno padronizado** para o portal, mesmo com parceiros externos caóticos.
- Mostrar domínio de **arquitetura hexagonal**, **SOLID**, **Clean Code** e **resiliência** (Retry + Circuit Breaker).
- Facilitar a integração com um **Logic App** que orquestra criação, processamento e consulta de ativações.

---

## Visão geral do problema

Cenário do desafio:

- Portal de clientes escolhe um DID e solicita ativação.
- Dois parceiros externos, ambos com autenticação JWT via REST:

  - `BrasilConnect` → DIDs com prefixo `+55` (Brasil)  
  - `WorldTel` → DIDs internacionais (demais prefixos)

- Os parceiros retornam **status detalhados**, mas o portal hoje mostra só “em andamento”.
- Além disso, as respostas dos parceiros são **inconsistentes**:
  - às vezes objeto, às vezes array;
  - campos aninhados em alguns casos;
  - formatos diferentes entre si;
  - erros com estruturas diferentes.

Durante campanhas, existem picos de requisição com:

- respostas lentas,
- instabilidades intermitentes,
- status incorretos ou ausentes no portal.

O objetivo deste backend é:

- falar com os parceiros,
- **normalizar as respostas**,
- expor endpoints limpos para o portal,
- registrar histórico completo das ativações,
- e se comportar bem em cenários de erro e pico de carga.

---

## Arquitetura

A solução foi montada em **arquitetura hexagonal (ports & adapters)**, separando bem domínio, aplicação, infraestrutura e API.

### Estrutura da solution

- `DidActivation.Domain`
- `DidActivation.Application`
- `DidActivation.Infrastructure`
- `DidActivation.Api`

### Camadas

#### 1. Domain (`DidActivation.Domain`)

Responsável pelo **core de negócio**, sem dependência de infraestrutura.

Principais elementos:

- **Value Objects**
  - `Did`  
    - garante formato E.164 (ex: `+5511999999999`);
    - decide o provedor (`BrasilConnect` vs `WorldTel`) via prefixo.
  - `ActivationStatus`  
    - empacota o código de status interno + mensagem amigável.

- **Enums**
  - `ActivationStatusCode`  
    - `REQUESTED`, `PENDING_PARTNER`, `IN_PROGRESS`, `WAITING_VALIDATION`, `DONE`,  
      `FAILED`, `FAILED_PARTNER_UNAVAILABLE`.

- **Entidades**
  - `Activation`  
    - representa a ativação de um DID para um cliente;
    - guarda `Did`, `CustomerId`, `CampaignId`, `Provider`, `IdempotencyKey`;
    - armazena status atual e dados do parceiro.
  - `ActivationHistory`  
    - registra o histórico de mudanças de status, com `RawStatus` do parceiro e `TimestampUtc`.

- **Ports (interfaces)**
  - `IActivationRepository`
  - `IPartnerGateway`
  - `IStatusMappingEngine`
  - `IDateTimeProvider`

- **Exceções de domínio**
  - `DomainException`
  - `ValidationException`  
    usadas para validações de negócio (ex: DID inválido).

#### 2. Application (`DidActivation.Application`)

Camada de **casos de uso**, orquestra o domínio e expõe serviços para a API.

- **DTOs**
  - `RequestActivationDto`  
    payload de entrada do `POST /activations`.
  - `ActivationCreatedResponseDto`  
    resposta simplificada ao criar uma ativação (inclui link `self`).
  - `ActivationDetailResponseDto`  
    resposta detalhada para `GET /activations/{id}` (status + histórico).
  - `ActivationHistoryItemDto`  
    item do histórico de status.

- **Serviço de aplicação**
  - `IActivationService`
  - `ActivationService`
    - cria ativações com **idempotência** (`idempotencyKey` baseado em `customerId + DID`);
    - escolhe parceiro pelo prefixo do DID;
    - chama `IPartnerGateway` (que fala com os parceiros);
    - usa `IStatusMappingEngine` para normalizar os status;
    - atualiza `Activation` + `ActivationHistory`;
    - expõe os contratos limpos para a API.

#### 3. Infrastructure (`DidActivation.Infrastructure`)

Implementa os **adapters** para os ports do domínio:

- **Persistence**
  - `InMemoryActivationRepository`  
    - implementação em memória do `IActivationRepository`  
    - simples para fins de desafio / POC.

- **Time**
  - `SystemDateTimeProvider`  
    - implementação de `IDateTimeProvider` usando `DateTime.UtcNow`.

- **Status mapping**
  - `StatusMappingEngine`  
    - implementa `IStatusMappingEngine`;
    - mapeia status caóticos dos parceiros para `ActivationStatusCode` interno.  
    - Exemplo:
      - BrasilConnect: `PENDING`, `IN_PROGRESS`, `WAITING_VALIDATION`, `DONE`, `FAILED`.
      - WorldTel: `WAITING_VALIDATION`, `PROCESSING`, `COMPLETED`, `FAILED`.

- **Partners**
  - `PartnerGateway`  
    - implementação de `IPartnerGateway`;
    - simula chamadas aos parceiros com **contratos diferentes**:
      - BrasilConnect:
        - URL fictícia: `https://api.brasilconnect.com/v1/dids/{did}/activation`
        - respostas às vezes como **objeto**, às vezes como **array**;
      - WorldTel:
        - URL fictícia: `https://api.worldtel.com/api/activation`
        - respostas com campos **aninhados** (`currentStatus.code`, `currentStatus.description`).
    - sempre converte a resposta caótica para um `PartnerRawResponse`.

- **Resiliência**
  - `ResilientPartnerGateway`  
    - decorador de `IPartnerGateway` usando **Polly**:
      - `WaitAndRetryAsync` com backoff exponencial;
      - `CircuitBreakerAsync` abrindo após N falhas por um período.
    - em caso de **circuito aberto**, a API responde com erro padronizado:  
      `PARTNER_UNAVAILABLE` e HTTP 503.

- **Dependency Injection**
  - `DependencyInjection.AddDidActivation`  
    - registra todas as dependências no `IServiceCollection`:
      - repositório, status engine, time provider,
      - gateway + decorator resiliente,
      - serviço de aplicação.

#### 4. API (`DidActivation.Api`)

Exposição via HTTP usando ASP.NET Core.

- **Program**
  - registra controllers, Swagger e `AddDidActivation()`;
  - configura o middleware de erro customizado.

- **Controller**
  - `ActivationsController`
    - `POST /activations`
      - recebe `RequestActivationDto` (com `did`, `customerId`, `campaignId`, `idempotencyKey`);
      - chama `IActivationService.RequestActivationAsync`;
      - retorna `202 Accepted` com `ActivationCreatedResponseDto`.
    - `GET /activations/{activationId}`
      - retorna `ActivationDetailResponseDto` com:
        - status atual,
        - motivo,
        - erro (se houver),
        - histórico de mudanças de status;
      - retorna `404` com JSON padronizado caso não encontre.

- **Middleware**
  - `ErrorHandlingMiddleware`
    - intercepta exceções e devolve JSON uniforme:
      - `ValidationException` → 400 (erro de entrada);
      - outras `DomainException` → 422;
      - `BrokenCircuitException` (Polly) → 503 (`PARTNER_UNAVAILABLE`);
      - `OperationCanceledException` → 408;
      - qualquer outro erro → 500 (`UNEXPECTED_ERROR`).

---

## Fluxos pensados para o Logic App

Embora o desafio peça o fluxo no **Excalidraw** para o Logic App, o backend foi estruturado pra encaixar de forma natural nos seguintes cenários:

1. **CreateActivation (Public API)**
   - Logic App exposto via HTTP recebe requisição do portal;
   - chama `POST /activations` deste backend;
   - recebe `activationId` e `status` inicial (`PENDING_PARTNER`) + link de consulta.

2. **ProcessActivation (Worker / Partner Integration)**
   - Logic App/Worker coleta ativações pendentes;
   - chama o backend para acionar os parceiros (via `IPartnerGateway`);
   - normalização + mapeamento de status + histórico na camada de domínio.

3. **GetActivationStatus (Consulta Portal)**
   - Portal chama `GET /activations/{id}`;
   - backend retorna status detalhado, motivo, erro e histórico completo.

---

## Tecnologias utilizadas

- **.NET 9 / C#**
  - ASP.NET Core para a API.
- **Arquitetura Hexagonal**
  - Separação clara de Domain, Application, Infrastructure, Api.
- **SOLID / Clean Code**
  - Ports & Adapters, controllers finos, serviços de aplicação focados em caso de uso.
- **Polly**
  - Retry + Circuit Breaker no `ResilientPartnerGateway`.
- **Swagger**
  - Documentação interativa para testar os endpoints.

---

## Como rodar o projeto

Pré-requisitos:

- .NET 9 SDK instalado.

Na raiz da solution:

```bash
cd DidActivation.Api
dotnet run
