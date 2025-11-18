var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Go.Backend.Infrastructure.Options.BotModelOptions>(
    builder.Configuration.GetSection(Go.Backend.Infrastructure.Options.BotModelOptions.SectionName));

builder.Services.AddSingleton<Go.Backend.Domain.Services.GoRulesService>();
builder.Services.AddSingleton<Go.Backend.Domain.Abstractions.IGameRepository,
    Go.Backend.Infrastructure.Persistence.InMemoryGameRepository>();
builder.Services.AddSingleton<Go.Backend.Application.Abstractions.IGoBotEngine,
    Go.Backend.Infrastructure.Bot.StubGoBotEngine>();
builder.Services.AddScoped<Go.Backend.Application.Abstractions.IGameService,
    Go.Backend.Application.Services.GameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/games", async (Go.Backend.Application.Abstractions.IGameService gameService, CancellationToken ct) =>
{
    var game = await gameService.CreateGameAsync(ct);
    return Results.Created($"/api/games/{game.Id}", ToStateDto(game));
});

app.MapGet("/api/games/{id:guid}", async (Guid id, Go.Backend.Application.Abstractions.IGameService gameService, CancellationToken ct) =>
{
    var game = await gameService.GetGameAsync(id, ct);
    return game is null ? Results.NotFound() : Results.Ok(ToStateDto(game));
});

app.MapPost("/api/games/{id:guid}/moves",
    async (Guid id, MoveRequest request, Go.Backend.Application.Abstractions.IGameService gameService,
        CancellationToken ct) =>
    {
        if (!TryParseColor(request.Color, out var color))
        {
            return Results.BadRequest("Color must be 'black' or 'white'.");
        }

        var position = new Go.Backend.Domain.ValueObjects.Position(request.X, request.Y);
        var result = await gameService.PlayMoveAsync(id, position, color, ct);

        if (!result.Success)
        {
            return Results.BadRequest(result.Error);
        }

        var game = await gameService.GetGameAsync(id, ct);
        return Results.Ok(new MoveResponse(
            ToDto(position),
            result.Captured.Select(ToDto).ToArray(),
            game is null ? null : ToStateDto(game)
        ));
    });

app.MapPost("/api/games/{id:guid}/bot-move",
    async (Guid id, BotMoveRequest request, Go.Backend.Application.Abstractions.IGameService gameService,
        CancellationToken ct) =>
    {
        var game = await gameService.GetGameAsync(id, ct);
        if (game is null)
        {
            return Results.NotFound();
        }

        var color = request.Color is not null
            ? ParseColorOrDefault(request.Color, game.NextPlayer)
            : game.NextPlayer;

        var (moveResult, botMove) = await gameService.PlayBotMoveAsync(id, color, ct);
        if (!moveResult.Success)
        {
            return Results.BadRequest(moveResult.Error);
        }

        return Results.Ok(new MoveResponse(
            ToDto(botMove ?? default),
            moveResult.Captured.Select(ToDto).ToArray(),
            ToStateDto(game)
        ));
    });

app.Run();

static bool TryParseColor(string value, out Go.Backend.Domain.Enums.StoneColor color)
{
    color = value.Trim().ToLowerInvariant() switch
    {
        "black" => Go.Backend.Domain.Enums.StoneColor.Black,
        "white" => Go.Backend.Domain.Enums.StoneColor.White,
        _ => Go.Backend.Domain.Enums.StoneColor.Empty
    };

    return color != Go.Backend.Domain.Enums.StoneColor.Empty;
}

static Go.Backend.Domain.Enums.StoneColor ParseColorOrDefault(string value, Go.Backend.Domain.Enums.StoneColor @default) =>
    TryParseColor(value, out var color) ? color : @default;

static PositionDto ToDto(Go.Backend.Domain.ValueObjects.Position position) => new(position.X, position.Y);

static GameStateDto ToStateDto(Go.Backend.Domain.Entities.Game game)
{
    var boardRows = new string[Go.Backend.Domain.Entities.Board.Size];
    for (var x = 0; x < Go.Backend.Domain.Entities.Board.Size; x++)
    {
        var row = new char[Go.Backend.Domain.Entities.Board.Size];
        for (var y = 0; y < Go.Backend.Domain.Entities.Board.Size; y++)
        {
            row[y] = game.Board.Get(new Go.Backend.Domain.ValueObjects.Position(x, y)) switch
            {
                Go.Backend.Domain.Enums.StoneColor.Black => 'B',
                Go.Backend.Domain.Enums.StoneColor.White => 'W',
                _ => '.'
            };
        }

        boardRows[x] = new string(row);
    }

    return new GameStateDto(
        game.Id,
        Go.Backend.Domain.Entities.Board.Size,
        game.NextPlayer.ToString(),
        game.MoveNumber,
        game.IsFinished,
        game.Winner?.ToString(),
        game.BlackCaptures,
        game.WhiteCaptures,
        boardRows);
}

public record PositionDto(int X, int Y);

public record GameStateDto(Guid GameId, int Size, string NextPlayer, int MoveNumber, bool IsFinished,
    string? Winner, int BlackCaptures, int WhiteCaptures, IReadOnlyList<string> Board);

public record MoveRequest(int X, int Y, string Color);

public record BotMoveRequest(string? Color);

public record MoveResponse(PositionDto Move, IReadOnlyCollection<PositionDto> Captured, GameStateDto? State);
