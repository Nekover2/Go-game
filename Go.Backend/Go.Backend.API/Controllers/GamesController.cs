using Microsoft.AspNetCore.Mvc;
using Go.Backend.Application.DTOs;
using Go.Backend.Application.Services;

namespace Go.Backend.API.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly GameService _gameService;

        public GamesController(GameService gameService)
        {
            _gameService = gameService;
        }

        // POST /api/games
        // Tạo game mới
        [HttpPost]
        public async Task<IActionResult> CreateGame()
        {
            var gameDto = await _gameService.CreateGameAsync();
            // Trả về 201 Created kèm header Location
            return CreatedAtAction(nameof(GetGame), new { id = gameDto.GameId }, gameDto);
        }

        // GET /api/games/{id}
        // Lấy trạng thái game
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(Guid id)
        {
            var gameDto = await _gameService.GetGameAsync(id);
            if (gameDto == null) return NotFound(new { message = "Game not found" });
            return Ok(gameDto);
        }

        // POST /api/games/{id}/moves
        // Người chơi đi quân
        [HttpPost("{id}/moves")]
        public async Task<IActionResult> PlayMove(Guid id, [FromBody] MakeMoveRequest request)
        {
            try
            {
                var result = await _gameService.ProcessMoveAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Game not found" });
            }
            catch (ArgumentException ex) // Lỗi luật game (ko, suicide...)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // Game đã kết thúc
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/games/{id}/bot-move
        // Yêu cầu Bot đi quân
        [HttpPost("{id}/bot-move")]
        public async Task<IActionResult> AskBotMove(Guid id, [FromBody] BotMoveRequest request)
        {
            try
            {
                var result = await _gameService.ProcessBotMoveAsync(id, request.Color);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Game not found" });
            }
            catch (Exception ex)
            {
                // Lỗi chung hoặc lỗi AI
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}