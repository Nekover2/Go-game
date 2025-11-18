using Go.Backend.Domain.Entities;

namespace Go.Backend.Domain.Abstractions;

public interface IGameRepository
{
    Task AddAsync(Game game, CancellationToken cancellationToken);
    Task<Game?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Game game, CancellationToken cancellationToken);
}
