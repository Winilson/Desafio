using DidActivation.Domain.ValueObjects;

namespace DidActivation.Domain.Entities
{
    public sealed class Activation
    {
        public Guid Id { get; private set; }
        public Did Did { get; private set; }
        public string CustomerId { get; private set; }
        public string? CampaignId { get; private set; }
        public string Provider { get; private set; }
        public string IdempotencyKey { get; private set; }
        public ActivationStatus Status { get; private set; }
        public string? ProviderRequestId { get; private set; }
        public string? ErrorCode { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }
        public IReadOnlyCollection<ActivationHistory> History => _history.AsReadOnly();

        private readonly List<ActivationHistory> _history = new();

        private Activation() { }

        public Activation(
            Guid id,
            Did did,
            string customerId,
            string? campaignId,
            string provider,
            string idempotencyKey,
            ActivationStatus initialStatus,
            DateTime nowUtc)
        {
            Id = id;
            Did = did;
            CustomerId = customerId;
            CampaignId = campaignId;
            Provider = provider;
            IdempotencyKey = idempotencyKey;
            Status = initialStatus;
            CreatedAtUtc = nowUtc;
            UpdatedAtUtc = nowUtc;

            AppendHistory(initialStatus, null, nowUtc);
        }

        public void ApplyPartnerUpdate(
            ActivationStatus status,
            string? providerRequestId,
            string? errorCode,
            string? errorMessage,
            string rawStatus,
            DateTime nowUtc)
        {
            Status = status;
            ProviderRequestId = providerRequestId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            UpdatedAtUtc = nowUtc;

            AppendHistory(status, rawStatus, nowUtc);
        }

        private void AppendHistory(ActivationStatus status, string? rawStatus, DateTime timestampUtc)
        {
            _history.Add(new ActivationHistory(
                Guid.NewGuid(),
                Id,
                status.Code,
                status.Reason,
                rawStatus,
                timestampUtc));
        }
    }
}
