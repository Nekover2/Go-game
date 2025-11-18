using System.Collections.Concurrent;
using Go.Backend.Domain.Abstractions;
using Go.Backend.Domain.Entities;

namespace Go.Backend.Infrastructure.Persistence;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Task AddAsync(Game game, CancellationToken cancellationToken)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }

    public Task<Game?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        _games.TryGetValue(id, out var game);
        return Task.FromResult(game);
    }

    public Task UpdateAsync(Game game, CancellationToken cancellationToken)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }
}
