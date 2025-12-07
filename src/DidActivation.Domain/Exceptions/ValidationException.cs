namespace DidActivation.Domain.Exceptions
{
    public sealed class ValidationException : DomainException
    {
        public ValidationException(string errorCode, string message)
            : base(errorCode, message)
        {
        }
    }
}
