using DidActivation.Domain.Enums;

namespace DidActivation.Domain.Entities
{
    public sealed class ActivationHistory
    {
        public Guid Id { get; }
        public Guid ActivationId { get; }
        public ActivationStatusCode StatusCode { get; }
        public string StatusReason { get; }
        public string? RawStatus { get; }
        public DateTime TimestampUtc { get; }

        public ActivationHistory(
            Guid id,
            Guid activationId,
            ActivationStatusCode statusCode,
            string statusReason,
            string? rawStatus,
            DateTime timestampUtc)
        {
            Id = id;
            ActivationId = activationId;
            StatusCode = statusCode;
            StatusReason = statusReason;
            RawStatus = rawStatus;
            TimestampUtc = timestampUtc;
        }
    }
}
