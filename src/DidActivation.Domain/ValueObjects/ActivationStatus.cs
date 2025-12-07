using DidActivation.Domain.Enums;

namespace DidActivation.Domain.ValueObjects
{
    public sealed class ActivationStatus
    {
        public ActivationStatusCode Code { get; }
        public string Reason { get; }

        public ActivationStatus(ActivationStatusCode code, string reason)
        {
            Code = code;
            Reason = string.IsNullOrWhiteSpace(reason) ? code.ToString() : reason;
        }

        public override string ToString() => $"{Code}: {Reason}";
    }
}
