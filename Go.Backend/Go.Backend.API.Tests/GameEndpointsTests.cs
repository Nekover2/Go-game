using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Go.Backend.API.Tests;

public class GameEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGame_ReturnsCreatedWithId()
    {
        var response = await _client.PostAsync("/api/games", content: null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.GameId);
        Assert.Equal(19, created.Size);
        Assert.Equal("Black", created.NextPlayer);
        Assert.Equal(19, created.Board.Count);
    }

    [Fact]
    public async Task PlayMove_AppliesLegalMoveAndAdvancesTurn()
    {
        var game = await CreateGameAsync();

        var moveResponse = await _client.PostAsJsonAsync($"/api/games/{game.GameId}/moves",
            new MoveRequest(3, 3, "black", false));

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);

        var move = await moveResponse.Content.ReadFromJsonAsync<MoveResponse>();
        Assert.NotNull(move);
        Assert.NotNull(move!.Move);
        Assert.Equal(3, move.Move!.X);
        Assert.Equal(3, move.Move!.Y);

        // After a black move, next player should be White.
        Assert.NotNull(move.State);
        Assert.Equal("White", move.State!.NextPlayer);
        Assert.Equal(1, move.State.MoveNumber);
    }

    [Fact]
    public async Task PlayMove_OnOccupiedPoint_IsRejected()
    {
        var game = await CreateGameAsync();

        // First legal move.
        var first = await _client.PostAsJsonAsync($"/api/games/{game.GameId}/moves",
            new MoveRequest(0, 0, "black", false));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        // Attempt to play on the same point by White should fail.
        var second = await _client.PostAsJsonAsync($"/api/games/{game.GameId}/moves",
            new MoveRequest(0, 0, "white", false));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task TwoPasses_EndGame()
    {
        var game = await CreateGameAsync();

        var firstPass = await _client.PostAsJsonAsync($"/api/games/{game.GameId}/moves",
            new MoveRequest(0, 0, "black", true));
        Assert.Equal(HttpStatusCode.OK, firstPass.StatusCode);

        var secondPass = await _client.PostAsJsonAsync($"/api/games/{game.GameId}/moves",
            new MoveRequest(0, 0, "white", true));
        Assert.Equal(HttpStatusCode.OK, secondPass.StatusCode);

        var state = await secondPass.Content.ReadFromJsonAsync<MoveResponse>();
        Assert.NotNull(state);
        Assert.NotNull(state!.State);
        Assert.True(state.State!.IsFinished);
    }

    private async Task<GameStateDto> CreateGameAsync()
    {
        var response = await _client.PostAsync("/api/games", content: null);
        var game = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(game);
        return game!;
    }
}
