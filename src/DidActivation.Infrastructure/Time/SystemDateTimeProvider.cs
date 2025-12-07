using DidActivation.Domain.Ports;

namespace DidActivation.Infrastructure.Time
{
    public sealed class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
