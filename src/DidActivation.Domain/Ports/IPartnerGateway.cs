using DidActivation.Domain.Entities;
using DidActivation.Domain.Models;

namespace DidActivation.Domain.Ports
{
    public interface IPartnerGateway
    {
        Task<PartnerRawResponse> SendActivationRequestAsync(
            Activation activation,
            CancellationToken cancellationToken);
    }
}
