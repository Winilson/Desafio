using DidActivation.Domain.Entities;
using DidActivation.Domain.Ports;
using System.Collections.Concurrent;

namespace DidActivation.Infrastructure.Persistence
{
    public sealed class InMemoryActivationRepository : IActivationRepository
    {
        private readonly ConcurrentDictionary<Guid, Activation> _byId = new();
        private readonly ConcurrentDictionary<string, Guid> _byIdempotencyKey = new();

        public Task<Activation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _byId.TryGetValue(id, out var activation);
            return Task.FromResult<Activation?>(activation);
        }

        public Task<Activation?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
        {
            if (_byIdempotencyKey.TryGetValue(idempotencyKey, out var id) &&
                _byId.TryGetValue(id, out var activation))
                return Task.FromResult<Activation?>(activation);

            return Task.FromResult<Activation?>(null);
        }

        public Task AddAsync(Activation activation, CancellationToken cancellationToken)
        {
            _byId[activation.Id] = activation;
            _byIdempotencyKey[activation.IdempotencyKey] = activation.Id;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Activation activation, CancellationToken cancellationToken)
        {
            _byId[activation.Id] = activation;
            return Task.CompletedTask;
        }
    }
}

