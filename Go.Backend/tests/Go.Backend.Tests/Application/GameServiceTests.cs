using Xunit;
using Moq;
using Go.Backend.Application.Services;
using Go.Backend.Application.Interfaces;
using Go.Backend.Application.Models;
using Go.Backend.Application.DTOs;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using System.Threading.Tasks;
using System;

namespace Go.Backend.Tests.Application
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockRepo;
        private readonly Mock<IGoAiService> _mockAi;
        private readonly GameService _service;

        public GameServiceTests()
        {
            _mockRepo = new Mock<IGameRepository>();
            _mockAi = new Mock<IGoAiService>();
            _service = new GameService(_mockRepo.Object, _mockAi.Object);
        }

        [Fact]
        public async Task CreateGame_ShouldReturnNewId()
        {
            // Arrange
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<GameMatch>()))
                     .ReturnsAsync((GameMatch g) => g); // Giả lập DB trả về chính game đó

            // Act
            var result = await _service.CreateGameAsync();

            // Assert
            Assert.NotEqual(Guid.Empty, result.GameId);
            Assert.Equal("Black", result.NextPlayer);
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<GameMatch>()), Times.Once);
        }

        [Fact]
        public async Task ProcessMove_ValidMove_ShouldSaveGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var gameMatch = new GameMatch(19);
            _mockRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(gameMatch);

            var request = new MakeMoveRequest { X = 3, Y = 3, Color = "Black" };

            // Act
            var response = await _service.ProcessMoveAsync(gameId, request);

            // Assert
            Assert.NotNull(response.Move);
            Assert.Equal(3, response.Move.X);
            Assert.Equal(PlayerColor.White, gameMatch.NextPlayer); // Phải đổi lượt
            _mockRepo.Verify(r => r.SaveAsync(gameMatch), Times.Once);
        }

        [Fact]
        public async Task ProcessBotMove_ShouldCallAiAndPlay()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var gameMatch = new GameMatch(19);
            // Đến lượt Trắng (Bot)
            gameMatch.RegisterMoveSuccess(0); // Đen đã đi 1 nước, giờ là Trắng
            
            _mockRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(gameMatch);
            
            // Giả lập AI trả về nước đi (4,4)
            _mockAi.Setup(ai => ai.GetBestMoveAsync(It.IsAny<Board>(), PlayerColor.White))
                   .ReturnsAsync(new MoveCoordinateDto { X = 4, Y = 4 });

            // Act
            var response = await _service.ProcessBotMoveAsync(gameId, null);

            // Assert
            Assert.Equal(4, response.Move.X);
            Assert.Equal(4, response.Move.Y);
            _mockAi.Verify(ai => ai.GetBestMoveAsync(It.IsAny<Board>(), PlayerColor.White), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<GameMatch>()), Times.Once);
        }
    }
}