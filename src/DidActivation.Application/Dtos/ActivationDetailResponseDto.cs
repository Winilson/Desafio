namespace DidActivation.Application.Dtos
{
    public sealed class ActivationDetailResponseDto
    {
        public Guid ActivationId { get; init; }
        public string Did { get; init; } = string.Empty;
        public string Provider { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string StatusReason { get; init; } = string.Empty;
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
        public IReadOnlyCollection<ActivationHistoryItemDto> History { get; init; } = Array.Empty<ActivationHistoryItemDto>();
    }
}
