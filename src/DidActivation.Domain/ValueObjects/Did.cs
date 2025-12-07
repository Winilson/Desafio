using DidActivation.Domain.Exceptions;

namespace DidActivation.Domain.ValueObjects
{
    public sealed class Did : IEquatable<Did>
    {
        public string Value { get; }

        public Did(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("INVALID_DID", "DID is required.");

            if (!value.StartsWith("+"))
                throw new ValidationException("INVALID_DID_FORMAT", "DID must be in E.164 format.");

            Value = value;
        }

        public bool IsBrazilian() => Value.StartsWith("+55");

        public override string ToString() => Value;

        public bool Equals(Did? other) => other is not null && Value == other.Value;

        public override bool Equals(object? obj) => obj is Did other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    }
}
