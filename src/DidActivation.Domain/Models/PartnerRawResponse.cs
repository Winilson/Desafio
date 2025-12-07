namespace DidActivation.Domain.Models
{
    public sealed class PartnerRawResponse
    {
        public string Provider { get; }
        public string? ProviderRequestId { get; }
        public string RawStatus { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }
        public string RawPayloadJson { get; }

        public PartnerRawResponse(
            string provider,
            string? providerRequestId,
            string rawStatus,
            string? errorCode,
            string? errorMessage,
            string rawPayloadJson)
        {
            Provider = provider;
            ProviderRequestId = providerRequestId;
            RawStatus = rawStatus;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            RawPayloadJson = rawPayloadJson;
        }
    }
}
