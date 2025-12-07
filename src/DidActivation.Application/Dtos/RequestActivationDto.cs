namespace DidActivation.Application.Dtos
{
    public sealed class RequestActivationDto
    {
        public string Did { get; init; } = string.Empty;
        public string CustomerId { get; init; } = string.Empty;
        public string? CampaignId { get; init; }
        public string? IdempotencyKey { get; init; }
    }
}
