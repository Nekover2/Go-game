using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Go.Backend.Application.DTOs;
using Go.Backend.Application.Interfaces;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Infrastructure.AI.Mcts;

namespace Go.Backend.Infrastructure.AI
{
    public class OnnxGoAiService : IGoAiService
    {
        private readonly InferenceSession _session;
        private const int SIMULATIONS = 400; // Số lần mô phỏng (Tăng lên 800-1600 nếu máy mạnh)

        public OnnxGoAiService(string modelPath)
        {
            try 
            {
                // Load model 1 lần duy nhất
                _session = new InferenceSession(modelPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load ONNX model at {modelPath}. Error: {ex.Message}");
            }
        }

        public Task<MoveCoordinateDto?> GetBestMoveAsync(Board board, PlayerColor aiColor)
        {
            // Chạy trong Task.Run để không chặn main thread
            return Task.Run(() => 
            {
                // 1. Khởi tạo MCTS Root
                var root = new MctsNode(null, -1, 1.0f);
                
                // Mở rộng lần đầu cho Root
                ExpandNode(root, board, aiColor);

                // 2. Chạy vòng lặp mô phỏng
                for (int i = 0; i < SIMULATIONS; i++)
                {
                    var node = root;
                    var simBoard = board.Clone(); // Clone để đi thử trên bàn cờ ảo
                    var simColor = aiColor;       // Theo dõi lượt đi trong mô phỏng

                    // A. Selection
                    while (!node.IsLeaf)
                    {
                        node = node.SelectBestChild();
                        // Thực hiện nước đi trên bàn cờ ảo
                        if (node.MoveIndex != 361) // Nếu không phải Pass
                        {
                            int x = node.MoveIndex / 19;
                            int y = node.MoveIndex % 19;
                            simBoard.PlayMove(x, y, simColor);
                        }
                        simColor = simColor.Opponent(); // Đổi lượt
                    }

                    // B. Expansion & Evaluation (Gọi ONNX)
                    // Lưu ý: Value trả về từ ONNX là từ góc nhìn của simColor
                    var (policy, value) = RunInference(simBoard, simColor);

                    // Nếu game chưa kết thúc thì mở rộng cây
                    // (Ở đây ta tạm bỏ qua check IsGameOver phức tạp để đơn giản hóa)
                    ExpandNode(node, simBoard, simColor, policy);

                    // C. Backpropagation
                    // Value cần được đảo dấu mỗi khi quay ngược lên cây
                    Backpropagate(node, value);
                }

                // 3. Chọn kết quả
                if (root.Children.Count == 0) return null; // Nên Pass
                
                var bestNode = root.GetMostVisitedChild();
                
                if (bestNode.MoveIndex == 361) return null; // Pass

                return new MoveCoordinateDto 
                { 
                    X = bestNode.MoveIndex / 19, 
                    Y = bestNode.MoveIndex % 19 
                };
            });
        }

        private void ExpandNode(MctsNode node, Board board, PlayerColor color, float[]? policy = null)
        {
            // Nếu policy null nghĩa là lần đầu gọi (tại root), cần chạy inference
            if (policy == null)
            {
                var result = RunInference(board, color);
                policy = result.policy;
            }

            // Tạo các nút con cho các nước đi hợp lệ
            // Duyệt qua tất cả 361 điểm + Pass
            for (int move = 0; move < 362; move++)
            {
                // Lọc nước đi hợp lệ (chỉ tạo node con nếu đi được)
                bool isValid = true;
                if (move < 361)
                {
                    int x = move / 19;
                    int y = move % 19;
                    // Kiểm tra cơ bản: ô trống (Domain Board.PlayMove sẽ check kỹ hơn nhưng chậm)
                    // Ở đây check nhanh để lọc bớt
                    if (board.Stones[x, y] != PlayerColor.None) isValid = false;
                    
                    // Nâng cao: Có thể gọi board.Clone().PlayMove(...) để check chính xác
                    // nhưng sẽ rất chậm. MCTS thường dựa vào xác suất Policy thấp để lờ đi nước sai.
                }

                if (isValid)
                {
                    node.Children.Add(new MctsNode(node, move, policy[move]));
                }
            }
        }

        private void Backpropagate(MctsNode node, float value)
        {
            var current = node;
            while (current != null)
            {
                current.VisitCount++;
                current.TotalValue += value;
                value = -value; // Đảo dấu cho đối thủ
                current = current.Parent;
            }
        }

        private (float[] policy, float value) RunInference(Board board, PlayerColor color)
        {
            var inputTensor = TensorHelper.GetInputTensor(board, color);
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            // Chạy ONNX
            // Lưu ý: Cần lock _session nếu chạy đa luồng, nhưng MCTS hiện tại đang chạy tuần tự
            using var results = _session.Run(inputs);

            var policy = results.First(r => r.Name == "policy").AsTensor<float>().ToArray();
            var value = results.First(r => r.Name == "value").AsTensor<float>().ToArray()[0];

            return (policy, value);
        }
    }
}