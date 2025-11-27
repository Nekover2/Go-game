namespace Go.Backend.Domain.Enums
{
    public enum PlayerColor
    {
        None = 0,   // Điểm trống
        Black = 1,  // Quân Đen (đi trước)
        White = 2   // Quân Trắng (đi sau)
    }

    public static class PlayerColorExtensions
    {
        // Hàm tiện ích để lấy màu đối thủ
        public static PlayerColor Opponent(this PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Black => PlayerColor.White,
                PlayerColor.White => PlayerColor.Black,
                _ => PlayerColor.None
            };
        }
    }
}