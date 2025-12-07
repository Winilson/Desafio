namespace DidActivation.Application.Dtos
{
    public sealed class ActivationCreatedResponseDto
    {
        public Guid ActivationId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Self { get; init; } = string.Empty;
    }
}
