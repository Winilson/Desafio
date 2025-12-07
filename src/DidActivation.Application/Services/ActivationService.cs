using DidActivation.Application.Dtos;
using DidActivation.Application.Interfaces;
using DidActivation.Domain.Entities;
using DidActivation.Domain.Enums;
using DidActivation.Domain.Ports;
using DidActivation.Domain.ValueObjects;

namespace DidActivation.Application.Services
{
    public sealed class ActivationService : IActivationService
    {
        private readonly IActivationRepository _repository;
        private readonly IPartnerGateway _partnerGateway;
        private readonly IStatusMappingEngine _statusMappingEngine;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ActivationService(
            IActivationRepository repository,
            IPartnerGateway partnerGateway,
            IStatusMappingEngine statusMappingEngine,
            IDateTimeProvider dateTimeProvider)
        {
            _repository = repository;
            _partnerGateway = partnerGateway;
            _statusMappingEngine = statusMappingEngine;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ActivationCreatedResponseDto> RequestActivationAsync(
            RequestActivationDto request,
            CancellationToken cancellationToken)
        {
            var did = new Did(request.Did);
            var provider = did.IsBrazilian() ? "BrasilConnect" : "WorldTel";

            var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
                ? $"{request.CustomerId}-{did.Value}"
                : request.IdempotencyKey;

            var existing = await _repository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing is not null)
                return MapToCreatedResponse(existing);

            var now = _dateTimeProvider.UtcNow;
            var initialStatus = new ActivationStatus(
                ActivationStatusCode.PENDING_PARTNER,
                "Activation requested to partner");

            var activation = new Activation(
                Guid.NewGuid(),
                did,
                request.CustomerId,
                request.CampaignId,
                provider,
                idempotencyKey,
                initialStatus,
                now);

            await _repository.AddAsync(activation, cancellationToken);

            var partnerResponse = await _partnerGateway.SendActivationRequestAsync(activation, cancellationToken);
            var mappedStatus = _statusMappingEngine.Map(
                partnerResponse.Provider,
                partnerResponse.RawStatus,
                partnerResponse.ErrorCode,
                partnerResponse.ErrorMessage);

            activation.ApplyPartnerUpdate(
                mappedStatus,
                partnerResponse.ProviderRequestId,
                partnerResponse.ErrorCode,
                partnerResponse.ErrorMessage,
                partnerResponse.RawStatus,
                _dateTimeProvider.UtcNow);

            await _repository.UpdateAsync(activation, cancellationToken);

            return MapToCreatedResponse(activation);
        }

        public async Task<ActivationDetailResponseDto?> GetActivationAsync(
            Guid activationId,
            CancellationToken cancellationToken)
        {
            var activation = await _repository.GetByIdAsync(activationId, cancellationToken);
            return activation is null ? null : MapToDetailResponse(activation);
        }

        private static ActivationCreatedResponseDto MapToCreatedResponse(Activation activation)
        {
            return new ActivationCreatedResponseDto
            {
                ActivationId = activation.Id,
                Status = activation.Status.Code.ToString(),
                Message = "Solicitação de ativação recebida",
                Self = $"/activations/{activation.Id}"
            };
        }

        private static ActivationDetailResponseDto MapToDetailResponse(Activation activation)
        {
            return new ActivationDetailResponseDto
            {
                ActivationId = activation.Id,
                Did = activation.Did.Value,
                Provider = activation.Provider,
                Status = activation.Status.Code.ToString(),
                StatusReason = activation.Status.Reason,
                ErrorCode = activation.ErrorCode,
                ErrorMessage = activation.ErrorMessage,
                CreatedAtUtc = activation.CreatedAtUtc,
                UpdatedAtUtc = activation.UpdatedAtUtc,
                History = activation.History
                    .OrderBy(h => h.TimestampUtc)
                    .Select(h => new ActivationHistoryItemDto
                    {
                        Status = h.StatusCode.ToString(),
                        StatusReason = h.StatusReason,
                        RawStatus = h.RawStatus,
                        TimestampUtc = h.TimestampUtc
                    })
                    .ToArray()
            };
        }
    }
}
