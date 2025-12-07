using DidActivation.Domain.Entities;

namespace DidActivation.Domain.Ports
{
    public interface IActivationRepository
    {
        Task<Activation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Activation?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
        Task AddAsync(Activation activation, CancellationToken cancellationToken);
        Task UpdateAsync(Activation activation, CancellationToken cancellationToken);
    }
}
