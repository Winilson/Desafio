namespace DidActivation.Domain.Ports
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
