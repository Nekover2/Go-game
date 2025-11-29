using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Go.Backend.Infrastructure.AI;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;

namespace Go.Backend.Tests.Infrastructure
{
    public class OnnxAiTests
    {
        // Đường dẫn file model (đã được copy ra thư mục bin/Debug/net10.0 nhờ bước 1)
        private const string ModelPath = "GoModel.onnx";

        [Fact]
        public async Task AI_Should_Generate_Valid_Move_On_Empty_Board()
        {
            // 1. Kiểm tra xem file model có tồn tại không
            if (!File.Exists(ModelPath))
            {
                // Nếu không có file, ta có thể Skip test hoặc Fail.
                // Ở đây fail để nhắc bạn copy file.
                Assert.Fail($"Model file not found at: {Path.GetFullPath(ModelPath)}. Please copy GoModel.onnx to the test project.");
            }

            // Arrange
            // Khởi tạo Service với đường dẫn model thực
            var aiService = new OnnxGoAiService(ModelPath);
            
            // Tạo một bàn cờ trống 19x19
            var board = new Board(19);

            // Act
            // Yêu cầu AI đi quân Đen (Black)
            var move = await aiService.GetBestMoveAsync(board, PlayerColor.Black);

            // Assert
            Assert.NotNull(move); // AI phải trả về nước đi (không được null lỗi)
            
            // Tọa độ phải hợp lệ (0-18)
            Assert.True(move.X >= 0 && move.X < 19, $"X coordinate {move.X} out of bounds");
            Assert.True(move.Y >= 0 && move.Y < 19, $"Y coordinate {move.Y} out of bounds");

            // In ra kết quả (Optional - xUnit không hiện Console.Write trừ khi cấu hình thêm)
            // System.Diagnostics.Debug.WriteLine($"AI chose: ({move.X}, {move.Y})");
        }

        [Fact]
        public async Task AI_Should_Not_Play_On_Occupied_Spot()
        {
            if (!File.Exists(ModelPath)) return;

            // Arrange
            var aiService = new OnnxGoAiService(ModelPath);
            var board = new Board(19);

            // Giả sử ta lấp đầy bàn cờ, chỉ chừa lại 1 ô duy nhất (0,0)
            // Lưu ý: Đây là test logic MCTS lọc nước đi hợp lệ
            // Để test nhanh, ta chỉ đặt quân vào khu vực trung tâm (Hoshi) xem AI có tránh không
            
            // Đặt quân Đen vào (3,3) - Sao
            board.PlayMove(3, 3, PlayerColor.Black);

            // Act
            // Hỏi AI (cầm Trắng) đi đâu
            var move = await aiService.GetBestMoveAsync(board, PlayerColor.White);

            // Assert
            Assert.NotNull(move);
            // Nước đi AI chọn KHÔNG ĐƯỢC trùng với (3,3)
            Assert.False(move.X == 3 && move.Y == 3, "AI played on an occupied spot!");
        }
    }
}