using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Models;
using Go.Backend.Domain.Services;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Entities;

public class Game
{
    private readonly HashSet<string> _previousSignatures;

    public Guid Id { get; }
    public Board Board { get; private set; }
    public StoneColor NextPlayer { get; private set; }
    public int MoveNumber { get; private set; }
    public bool IsFinished { get; private set; }
    public StoneColor? Winner { get; private set; }
    public int BlackCaptures { get; private set; }
    public int WhiteCaptures { get; private set; }

    private Game(Guid id, Board board, StoneColor nextPlayer, int moveNumber, bool isFinished, StoneColor? winner,
        HashSet<string> previousSignatures, int blackCaptures, int whiteCaptures)
    {
        Id = id;
        Board = board;
        NextPlayer = nextPlayer;
        MoveNumber = moveNumber;
        IsFinished = isFinished;
        Winner = winner;
        _previousSignatures = previousSignatures;
        BlackCaptures = blackCaptures;
        WhiteCaptures = whiteCaptures;
    }

    public static Game CreateNew()
    {
        var board = new Board();
        var previous = new HashSet<string> { board.BuildSignature(StoneColor.Black) };
        return new Game(Guid.NewGuid(), board, StoneColor.Black, 0, false, null, previous, 0, 0);
    }

    public MoveResult PlayMove(Position position, StoneColor color, GoRulesService rules)
    {
        if (IsFinished)
        {
            return MoveResult.Failed("Game is already finished.");
        }

        if (color != NextPlayer)
        {
            return MoveResult.Failed($"It is {NextPlayer} to move.");
        }

        var result = rules.TryPlayMove(this, position, color);
        if (!result.Success)
        {
            return result;
        }

        Board = result.BoardAfterMove ?? Board;
        MoveNumber++;
        NextPlayer = color.Opponent();

        if (result.Captured is { Count: > 0 })
        {
            if (color == StoneColor.Black)
            {
                BlackCaptures += result.Captured.Count;
            }
            else
            {
                WhiteCaptures += result.Captured.Count;
            }
        }

        _previousSignatures.Add(Board.BuildSignature(NextPlayer));
        return result;
    }

    public bool HasSeenSignature(string signature) => _previousSignatures.Contains(signature);

    public void FinishGame(StoneColor? winner)
    {
        IsFinished = true;
        Winner = winner;
    }
}
