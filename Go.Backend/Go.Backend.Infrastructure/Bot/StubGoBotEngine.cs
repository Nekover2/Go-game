using Go.Backend.Application.Abstractions;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Services;
using Go.Backend.Domain.ValueObjects;
using Go.Backend.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Go.Backend.Infrastructure.Bot;

public class StubGoBotEngine : IGoBotEngine
{
    private readonly ILogger<StubGoBotEngine> _logger;
    private readonly GoRulesService _rulesService;
    private readonly string _modelPath;
    private readonly byte[]? _modelBytes;

    public StubGoBotEngine(IOptions<BotModelOptions> options, ILogger<StubGoBotEngine> logger, GoRulesService rulesService)
    {
        _logger = logger;
        _rulesService = rulesService;
        _modelPath = options.Value.ModelPath;
        _modelBytes = LoadModel(_modelPath);
    }

    public Task<Position> SuggestMoveAsync(Game game, StoneColor botColor, CancellationToken cancellationToken)
    {
        var empties = game.Board.GetEmptyPositions().ToList();
        if (empties.Count == 0)
        {
            throw new InvalidOperationException("No legal moves available.");
        }

        // Simple heuristic: prefer center positions, skip obviously illegal moves.
        var center = Board.Size / 2;
        var ordered = empties
            .OrderBy(p => Math.Abs(p.X - center) + Math.Abs(p.Y - center))
            .ToList();

        foreach (var position in ordered)
        {
            var trial = _rulesService.TryPlayMove(game, position, botColor);
            if (trial.Success)
            {
                return Task.FromResult(position);
            }
        }

        // Fallback to the first empty point if everything else failed.
        return Task.FromResult(empties[0]);
    }

    private byte[]? LoadModel(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogWarning("Bot model path is empty; using heuristic move selection.");
            return null;
        }

        try
        {
            var bytes = File.Exists(path) ? File.ReadAllBytes(path) : null;
            if (bytes == null)
            {
                _logger.LogWarning("Bot model file not found at {Path}; using heuristic move selection.", path);
            }
            else
            {
                _logger.LogInformation("Loaded bot model from {Path} ({Bytes} bytes).", path, bytes.Length);
            }

            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load bot model from {Path}. Using heuristic moves instead.", path);
            return null;
        }
    }
}
