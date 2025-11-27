using System;
using System.Linq;
using System.Threading.Tasks;
using Go.Backend.Application.DTOs;
using Go.Backend.Application.Interfaces;
using Go.Backend.Application.Models;
using Go.Backend.Domain.Enums;

namespace Go.Backend.Application.Services
{
    public class GameService
    {
        private readonly IGameRepository _repository;
        private readonly IGoAiService _aiService;

        public GameService(IGameRepository repository, IGoAiService aiService)
        {
            _repository = repository;
            _aiService = aiService;
        }

        public async Task<GameStateDto> CreateGameAsync()
        {
            var game = new GameMatch(19);
            await _repository.CreateAsync(game);
            return MapToDto(game);
        }

        public async Task<GameStateDto?> GetGameAsync(Guid id)
        {
            var game = await _repository.GetByIdAsync(id);
            return game == null ? null : MapToDto(game);
        }

        public async Task<MoveResponseDto> ProcessMoveAsync(Guid gameId, MakeMoveRequest request)
        {
            var game = await _repository.GetByIdAsync(gameId);
            if (game == null) throw new KeyNotFoundException("Game not found");
            if (game.IsFinished) throw new InvalidOperationException("Game is finished");

            // Validate lượt đi
            var requestColor = Enum.Parse<PlayerColor>(request.Color, true);
            if (requestColor != game.NextPlayer)
                throw new ArgumentException("Not your turn");

            MoveCoordinateDto? playedMove = null;
            var capturedCoords = new List<MoveCoordinateDto>();

            if (request.Pass)
            {
                game.ApplyPass();
            }
            else
            {
                // Gọi Domain để xử lý logic bàn cờ
                var result = game.Board.PlayMove(request.X, request.Y, requestColor);
                
                if (!result.IsSuccess)
                    throw new ArgumentException(result.ErrorMessage);

                game.RegisterMoveSuccess(result.CapturedStones.Count);
                
                playedMove = new MoveCoordinateDto { X = request.X, Y = request.Y };
                capturedCoords = result.CapturedStones
                    .Select(c => new MoveCoordinateDto { X = c.X, Y = c.Y })
                    .ToList();
            }

            await _repository.SaveAsync(game);

            return new MoveResponseDto
            {
                Move = playedMove,
                Captured = capturedCoords,
                State = MapToDto(game)
            };
        }

        public async Task<MoveResponseDto> ProcessBotMoveAsync(Guid gameId, string? requestedColor)
        {
            var game = await _repository.GetByIdAsync(gameId);
            if (game == null) throw new KeyNotFoundException("Game not found");
            if (game.IsFinished) throw new InvalidOperationException("Game finished");

            var aiColor = requestedColor != null 
                ? Enum.Parse<PlayerColor>(requestedColor, true) 
                : game.NextPlayer;

            // 1. Gọi AI Service (Interface)
            var bestMove = await _aiService.GetBestMoveAsync(game.Board, aiColor);

            // 2. Tái sử dụng logic đi quân
            var moveRequest = new MakeMoveRequest
            {
                Color = aiColor.ToString(),
                Pass = (bestMove == null),
                X = bestMove?.X ?? 0,
                Y = bestMove?.Y ?? 0
            };

            return await ProcessMoveAsync(gameId, moveRequest);
        }

        private GameStateDto MapToDto(GameMatch game)
        {
            return new GameStateDto
            {
                GameId = game.Id,
                Size = game.Board.Size,
                NextPlayer = game.NextPlayer.ToString(),
                MoveNumber = game.MoveNumber,
                IsFinished = game.IsFinished,
                Winner = game.Winner?.ToString(),
                BlackCaptures = game.BlackCaptures,
                WhiteCaptures = game.WhiteCaptures,
                Board = game.GetBoardStringArray()
            };
        }
    }
}