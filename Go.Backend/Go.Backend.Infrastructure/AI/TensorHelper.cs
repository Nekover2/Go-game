using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Go.Backend.Infrastructure.AI
{
    public static class TensorHelper
    {
        public static DenseTensor<float> GetInputTensor(Board board, PlayerColor aiColor)
        {
            // Shape: [Batch=1, Channels=17, Height=19, Width=19]
            var tensor = new DenseTensor<float>(new[] { 1, 17, 19, 19 });
            var opponent = aiColor.Opponent();

            // 17 Kênh: 
            // 0-7: Quân ta (hiện tại -> quá khứ)
            // 8-15: Quân địch (hiện tại -> quá khứ)
            // 16: Màu quân (Toàn 1 nếu Đen, 0 nếu Trắng)

            for (int y = 0; y < 19; y++)
            {
                for (int x = 0; x < 19; x++)
                {
                    var stone = board.Stones[x, y];

                    // Kênh 0: Quân Ta hiện tại
                    if (stone == aiColor) tensor[0, 0, x, y] = 1.0f;
                    
                    // Kênh 8: Quân Địch hiện tại
                    if (stone == opponent) tensor[0, 8, x, y] = 1.0f;

                    // Kênh 16: Màu quân (Lượt đi)
                    // Nếu AI cầm Đen -> 1, Trắng -> 0
                    if (aiColor == PlayerColor.Black) tensor[0, 16, x, y] = 1.0f;
                    
                    // TODO: Sau này nếu Board lưu lịch sử, hãy điền vào kênh 1-7 và 9-15
                }
            }

            return tensor;
        }
    }
}