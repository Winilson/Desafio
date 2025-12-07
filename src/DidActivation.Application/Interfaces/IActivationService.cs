using DidActivation.Application.Dtos;

namespace DidActivation.Application.Interfaces
{
    public interface IActivationService
    {
        Task<ActivationCreatedResponseDto> RequestActivationAsync(
            RequestActivationDto request,
            CancellationToken cancellationToken);

        Task<ActivationDetailResponseDto?> GetActivationAsync(
            Guid activationId,
            CancellationToken cancellationToken);
    }
}
