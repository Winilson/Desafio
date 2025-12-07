using DidActivation.Domain.ValueObjects;

namespace DidActivation.Domain.Ports
{
    public interface IStatusMappingEngine
    {
        ActivationStatus Map(string provider, string rawStatus, string? errorCode, string? errorMessage);
    }
}
