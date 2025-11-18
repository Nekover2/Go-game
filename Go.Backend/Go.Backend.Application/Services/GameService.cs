using Go.Backend.Application.Abstractions;
using Go.Backend.Domain.Abstractions;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Models;
using Go.Backend.Domain.Services;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly GoRulesService _rulesService;
    private readonly IGoBotEngine _botEngine;

    public GameService(IGameRepository repository, GoRulesService rulesService, IGoBotEngine botEngine)
    {
        _repository = repository;
        _rulesService = rulesService;
        _botEngine = botEngine;
    }

    public async Task<Game> CreateGameAsync(CancellationToken cancellationToken)
    {
        var game = Game.CreateNew();
        await _repository.AddAsync(game, cancellationToken);
        return game;
    }

    public Task<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken) =>
        _repository.GetAsync(id, cancellationToken);

    public async Task<MoveResult> PlayMoveAsync(Guid gameId, Position position, StoneColor color, bool isPass, CancellationToken cancellationToken)
    {
        var game = await _repository.GetAsync(gameId, cancellationToken);
        if (game is null)
        {
            return MoveResult.Failed("Game not found.");
        }

        var result = isPass
            ? game.Pass(color)
            : game.PlayMove(position, color, _rulesService);
        if (result.Success)
        {
            await _repository.UpdateAsync(game, cancellationToken);
        }

        return result;
    }

    public async Task<(MoveResult move, Position? botMove)> PlayBotMoveAsync(Guid gameId, StoneColor color, CancellationToken cancellationToken)
    {
        var game = await _repository.GetAsync(gameId, cancellationToken);
        if (game is null)
        {
            return (MoveResult.Failed("Game not found."), null);
        }

        var suggestion = await _botEngine.SuggestMoveAsync(game, color, cancellationToken);
        var result = game.PlayMove(suggestion, color, _rulesService);

        if (result.Success)
        {
            await _repository.UpdateAsync(game, cancellationToken);
            return (result, suggestion);
        }

        return (result, suggestion);
    }
}
