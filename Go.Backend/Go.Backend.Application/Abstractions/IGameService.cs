using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Models;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Application.Abstractions;

public interface IGameService
{
    Task<Game> CreateGameAsync(CancellationToken cancellationToken);
    Task<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken);
    Task<MoveResult> PlayMoveAsync(Guid gameId, Position position, StoneColor color, CancellationToken cancellationToken);
    Task<(MoveResult move, Position? botMove)> PlayBotMoveAsync(Guid gameId, StoneColor color, CancellationToken cancellationToken);
}
