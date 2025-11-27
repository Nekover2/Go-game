using System.Threading.Tasks;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;
using Go.Backend.Application.DTOs;

namespace Go.Backend.Application.Interfaces
{
    public interface IGoAiService
    {
        // Input: Bàn cờ hiện tại và màu quân AI cầm
        // Output: Tọa độ nước đi tối ưu (hoặc null nếu Pass)
        Task<MoveCoordinateDto?> GetBestMoveAsync(Board board, PlayerColor aiColor);
    }
}