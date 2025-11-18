using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Application.Abstractions;

public interface IGoBotEngine
{
    Task<Position> SuggestMoveAsync(Game game, StoneColor botColor, CancellationToken cancellationToken);
}
