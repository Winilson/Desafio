using DidActivation.Domain.Entities;
using DidActivation.Domain.Models;
using DidActivation.Domain.Ports;
using Polly;
using Polly.Wrap;

namespace DidActivation.Infrastructure.Resilience
{
    public sealed class ResilientPartnerGateway : IPartnerGateway
    {
        private readonly IPartnerGateway _inner;
        private readonly AsyncPolicyWrap<PartnerRawResponse> _policy;

        public ResilientPartnerGateway(IPartnerGateway inner)
        {
            _inner = inner;
            _policy = BuildPolicy();
        }

        public Task<PartnerRawResponse> SendActivationRequestAsync(
            Activation activation,
            CancellationToken cancellationToken)
        {
            return _policy.ExecuteAsync(
                ct => _inner.SendActivationRequestAsync(activation, ct),
                cancellationToken);
        }

        private static AsyncPolicyWrap<PartnerRawResponse> BuildPolicy()
        {
            var retry = Policy<PartnerRawResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            var circuitBreaker = Policy<PartnerRawResponse>
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    5,
                    TimeSpan.FromSeconds(30));

            return Policy.WrapAsync(retry, circuitBreaker);
        }
    }
}
