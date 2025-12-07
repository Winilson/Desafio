using DidActivation.Domain.Entities;
using DidActivation.Domain.Models;
using DidActivation.Domain.Ports;
using System.Text.Json;

namespace DidActivation.Infrastructure.Partner
{
    public sealed class PartnerGateway : IPartnerGateway
    {
        public Task<PartnerRawResponse> SendActivationRequestAsync(
            Activation activation,
            CancellationToken cancellationToken)
        {
            return activation.Provider switch
            {
                "BrasilConnect" => Task.FromResult(SimulateBrasilConnect(activation)),
                "WorldTel" => Task.FromResult(SimulateWorldTel(activation)),
                _ => Task.FromResult(new PartnerRawResponse(
                    activation.Provider,
                    null,
                    "FAILED",
                    "UNKNOWN_PROVIDER",
                    "Unknown provider",
                    "{}"))
            };
        }

        private static PartnerRawResponse SimulateBrasilConnect(Activation activation)
        {
            var url = $"https://api.brasilconnect.com/v1/dids/{activation.Did.Value}/activation";
            var jwt = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

            var asObject = new
            {
                requestId = "BR-987",
                status = "PENDING",
                message = "Activation request received"
            };

            var payloadObject = JsonSerializer.Serialize(asObject);

            var asArray = new[]
            {
            new { step = "VALIDATION", state = "IN_PROGRESS", timestamp = DateTime.UtcNow },
            new { step = "PROVISIONING", state = "PENDING", timestamp = DateTime.UtcNow }
        };

            var payloadArray = JsonSerializer.Serialize(asArray);

            var useArray = activation.Id.GetHashCode() % 2 == 0;

            var rawStatus = useArray ? "PENDING" : "PENDING";
            var rawPayload = useArray ? payloadArray : payloadObject;

            return new PartnerRawResponse(
                "BrasilConnect",
                "BR-987",
                rawStatus,
                null,
                null,
                rawPayload);
        }

        private static PartnerRawResponse SimulateWorldTel(Activation activation)
        {
            var url = "https://api.worldtel.com/api/activation";
            var jwt = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

            var asNested = new
            {
                id = "WT-555",
                currentStatus = new
                {
                    code = "WAITING_VALIDATION",
                    description = "Waiting user document validation"
                }
            };

            var payload = JsonSerializer.Serialize(asNested);

            return new PartnerRawResponse(
                "WorldTel",
                "WT-555",
                "WAITING_VALIDATION",
                null,
                null,
                payload);
        }
    }
}
