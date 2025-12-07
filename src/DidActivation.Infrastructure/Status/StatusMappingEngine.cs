using DidActivation.Domain.Enums;
using DidActivation.Domain.Ports;
using DidActivation.Domain.ValueObjects;

namespace DidActivation.Infrastructure.Status
{
    public sealed class StatusMappingEngine : IStatusMappingEngine
    {
        private readonly IReadOnlyDictionary<(string Provider, string RawStatus), ActivationStatus> _map;

        public StatusMappingEngine()
        {
            _map = BuildMap();
        }

        public ActivationStatus Map(string provider, string rawStatus, string? errorCode, string? errorMessage)
        {
            var key = (provider, rawStatus);

            if (_map.TryGetValue(key, out var status))
                return status;

            if (!string.IsNullOrWhiteSpace(errorCode) || !string.IsNullOrWhiteSpace(errorMessage))
                return new ActivationStatus(ActivationStatusCode.FAILED, errorMessage ?? errorCode ?? "Failed");

            return new ActivationStatus(ActivationStatusCode.IN_PROGRESS, "Unknown partner status");
        }

        private static IReadOnlyDictionary<(string Provider, string RawStatus), ActivationStatus> BuildMap()
        {
            var entries = new Dictionary<(string, string), ActivationStatus>
        {
            { ("BrasilConnect", "PENDING"), new ActivationStatus(ActivationStatusCode.PENDING_PARTNER, "Activation requested to partner") },
            { ("BrasilConnect", "IN_PROGRESS"), new ActivationStatus(ActivationStatusCode.IN_PROGRESS, "Processing in partner system") },
            { ("BrasilConnect", "WAITING_VALIDATION"), new ActivationStatus(ActivationStatusCode.WAITING_VALIDATION, "Waiting validation in partner system") },
            { ("BrasilConnect", "DONE"), new ActivationStatus(ActivationStatusCode.DONE, "Activation completed by partner") },
            { ("BrasilConnect", "FAILED"), new ActivationStatus(ActivationStatusCode.FAILED, "Activation failed in partner") },

            { ("WorldTel", "WAITING_VALIDATION"), new ActivationStatus(ActivationStatusCode.WAITING_VALIDATION, "Waiting user document validation") },
            { ("WorldTel", "PROCESSING"), new ActivationStatus(ActivationStatusCode.IN_PROGRESS, "Processing in WorldTel system") },
            { ("WorldTel", "COMPLETED"), new ActivationStatus(ActivationStatusCode.DONE, "Activation completed by WorldTel") },
            { ("WorldTel", "FAILED"), new ActivationStatus(ActivationStatusCode.FAILED, "Activation failed in WorldTel") }
        };

            return entries;
        }
    }
}
