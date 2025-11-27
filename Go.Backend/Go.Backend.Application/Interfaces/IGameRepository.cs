using System;
using System.Threading.Tasks;
using Go.Backend.Application.Models;
using Go.Backend.Domain.Entities; // Cần reference Domain

namespace Go.Backend.Application.Interfaces
{
    // Interface này sẽ được implement ở lớp Infrastructure (dùng EF Core hoặc InMemory)
    public interface IGameRepository
    {
        // Lưu ý: Tạm thời ta dùng object GameMatch (sẽ tạo ở dưới) để đại diện cho cả ván đấu
        Task<GameMatch?> GetByIdAsync(Guid id);
        Task SaveAsync(GameMatch game);
        Task<GameMatch> CreateAsync(GameMatch game);
    }
}