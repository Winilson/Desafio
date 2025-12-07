namespace DidActivation.Application.Dtos
{
    public sealed class ActivationHistoryItemDto
    {
        public string Status { get; init; } = string.Empty;
        public string StatusReason { get; init; } = string.Empty;
        public string? RawStatus { get; init; }
        public DateTime TimestampUtc { get; init; }
    }
}
