using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Models;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Services;

public class GoRulesService
{
    public MoveResult TryPlayMove(Game game, Position position, StoneColor color)
    {
        if (!position.IsInsideBoard())
        {
            return MoveResult.Failed("Move is outside the board.");
        }

        if (game.Board.Get(position) != StoneColor.Empty)
        {
            return MoveResult.Failed("Intersection is already occupied.");
        }

        var workingBoard = game.Board.Clone();
        workingBoard.PlaceStone(position, color);

        var captured = CaptureOpponentGroups(workingBoard, position, color);

        var ownGroup = workingBoard.CollectGroup(position);
        var liberties = workingBoard.CountLiberties(ownGroup);
        if (liberties == 0)
        {
            return MoveResult.Failed("Move is suicidal.");
        }

        var signature = workingBoard.BuildSignature(color.Opponent());
        if (game.HasSeenSignature(signature))
        {
            return MoveResult.Failed("Move violates ko (repeats a previous board state).");
        }

        return MoveResult.Ok(position, captured, workingBoard);
    }

    private static List<Position> CaptureOpponentGroups(Board board, Position position, StoneColor color)
    {
        var captured = new List<Position>();

        foreach (var neighbor in board.GetNeighbors(position))
        {
            if (board.Get(neighbor) != color.Opponent())
            {
                continue;
            }

            var group = board.CollectGroup(neighbor);
            if (board.CountLiberties(group) == 0)
            {
                captured.AddRange(group);
            }
        }

        foreach (var stone in captured)
        {
            board.RemoveStone(stone);
        }

        return captured;
    }
}
