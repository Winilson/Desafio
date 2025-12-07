namespace DidActivation.Domain.Enums
{
    public enum ActivationStatusCode
    {
        REQUESTED = 0,
        PENDING_PARTNER = 1,
        IN_PROGRESS = 2,
        WAITING_VALIDATION = 3,
        DONE = 4,
        FAILED = 5,
        FAILED_PARTNER_UNAVAILABLE = 6
    }
}
