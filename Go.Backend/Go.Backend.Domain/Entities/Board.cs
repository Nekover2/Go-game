using System;
using System.Collections.Generic;
using System.Text;
using Go.Backend.Domain.Enums;
using Go.Backend.Domain.ValueObjects;
using Go.Backend.Domain.Exceptions;

namespace Go.Backend.Domain.Entities
{
    public class Board
    {
        public int Size { get; private set; } = 19;
        public PlayerColor[,] Stones { get; private set; }
        
        // Hash của trạng thái bàn cờ trước đó (để kiểm tra luật Ko đơn giản)
        public string PreviousStateHash { get; private set; } = string.Empty;
        
        // Hash hiện tại (được tính toán động)
        public string CurrentStateHash => GenerateBoardHash();
        private Board() 
        {
            // Khởi tạo mặc định để tránh null, dù JSON sẽ ghi đè ngay sau đó
            Stones = new PlayerColor[19, 19]; 
        }

        public Board(int size = 19)
        {
            Size = size;
            Stones = new PlayerColor[size, size];
        }

        // Copy constructor (Dùng cho AI Clone ra bàn cờ ảo)
        public Board(Board other)
        {
            Size = other.Size;
            Stones = (PlayerColor[,])other.Stones.Clone();
            PreviousStateHash = other.PreviousStateHash;
        }

        public Board Clone() => new Board(this);

        /// <summary>
        /// Thực hiện một nước đi và trả về kết quả
        /// </summary>
        public MoveResult PlayMove(int x, int y, PlayerColor color)
        {
            // 1. Kiểm tra cơ bản
            if (!IsValidCoordinate(x, y))
                return MoveResult.Failure($"Tọa độ ({x},{y}) không hợp lệ.");
                
            if (Stones[x, y] != PlayerColor.None)
                return MoveResult.Failure("Ô đã có quân.");

            // Lưu trạng thái trước khi đi để revert nếu phạm luật
            var backupStones = (PlayerColor[,])Stones.Clone();
            string preMoveHash = CurrentStateHash;

            // 2. Đặt quân giả định
            Stones[x, y] = color;
            
            var opponent = color.Opponent();
            var capturedStones = new List<Coordinate>();

            // 3. Kiểm tra bắt quân đối thủ (4 hướng xung quanh)
            var neighbors = GetNeighbors(x, y);
            foreach (var neighbor in neighbors)
            {
                // Nếu cạnh bên là quân địch
                if (Stones[neighbor.X, neighbor.Y] == opponent)
                {
                    // Kiểm tra đám quân địch đó có còn khí không?
                    if (!HasLiberties(neighbor.X, neighbor.Y))
                    {
                        var groupToCapture = GetGroup(neighbor.X, neighbor.Y);
                        capturedStones.AddRange(groupToCapture);
                        RemoveGroup(groupToCapture); // Nhấc quân chết ra khỏi bàn
                    }
                }
            }

            // 4. Kiểm tra Tự sát (Suicide Rule)
            // Nếu nước đi không bắt được ai, mà chính mình lại hết khí -> Phạm luật
            if (!HasLiberties(x, y) && capturedStones.Count == 0)
            {
                Stones = backupStones; // Hoàn tác
                return MoveResult.Failure("Nước đi tự sát (Suicide) không hợp lệ.");
            }

            // 5. Kiểm tra luật Ko (Trạng thái lặp lại ngay lập tức)
            string newHash = GenerateBoardHash();
            if (newHash == PreviousStateHash)
            {
                Stones = backupStones; // Hoàn tác
                return MoveResult.Failure("Phạm luật Ko (Cướp). Bạn không được lặp lại trạng thái cũ ngay lập tức.");
            }

            // 6. Thành công: Cập nhật hash lịch sử
            PreviousStateHash = preMoveHash;
            
            return MoveResult.Success(capturedStones);
        }

        // --- CÁC THUẬT TOÁN HỖ TRỢ (PRIVATE) ---

        // Kiểm tra xem nhóm quân tại (x,y) có còn khí (ô trống cạnh bên) không
        public bool HasLiberties(int x, int y)
        {
            var color = Stones[x, y];
            // Dùng thuật toán Loang (BFS/DFS) để duyệt hết cả đám quân
            var visited = new HashSet<Coordinate>();
            var stack = new Stack<Coordinate>();
            
            stack.Push(new Coordinate(x, y));
            visited.Add(new Coordinate(x, y));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var neighbors = GetNeighbors(current.X, current.Y);

                foreach (var n in neighbors)
                {
                    var neighborColor = Stones[n.X, n.Y];
                    
                    // Nếu tìm thấy 1 ô trống -> Đám quân này SỐNG
                    if (neighborColor == PlayerColor.None) return true;

                    // Nếu là quân cùng màu chưa duyệt -> Thêm vào stack để duyệt tiếp
                    if (neighborColor == color && !visited.Contains(n))
                    {
                        visited.Add(n);
                        stack.Push(n);
                    }
                }
            }
            
            // Duyệt hết cả đám mà không thấy ô trống nào -> CHẾT
            return false;
        }

        // Lấy danh sách tọa độ của một đám quân (để nhấc ra khỏi bàn)
        private List<Coordinate> GetGroup(int x, int y)
        {
            var group = new List<Coordinate>();
            var color = Stones[x, y];
            var visited = new HashSet<Coordinate>();
            var stack = new Stack<Coordinate>();

            stack.Push(new Coordinate(x, y));
            visited.Add(new Coordinate(x, y));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                group.Add(current); // Thêm vào danh sách nhóm

                foreach (var n in GetNeighbors(current.X, current.Y))
                {
                    if (Stones[n.X, n.Y] == color && !visited.Contains(n))
                    {
                        visited.Add(n);
                        stack.Push(n);
                    }
                }
            }
            return group;
        }

        private void RemoveGroup(List<Coordinate> group)
        {
            foreach (var stone in group)
            {
                Stones[stone.X, stone.Y] = PlayerColor.None;
            }
        }

        private List<Coordinate> GetNeighbors(int x, int y)
        {
            var list = new List<Coordinate>(4);
            if (x > 0) list.Add(new Coordinate(x - 1, y));
            if (x < Size - 1) list.Add(new Coordinate(x + 1, y));
            if (y > 0) list.Add(new Coordinate(x, y - 1));
            if (y < Size - 1) list.Add(new Coordinate(x, y + 1));
            return list;
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }

        // Tạo chuỗi đại diện bàn cờ (để so sánh Ko)
        private string GenerateBoardHash()
        {
            var sb = new StringBuilder(Size * Size);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    sb.Append((int)Stones[i, j]);
                }
            }
            return sb.ToString();
        }
    }
}