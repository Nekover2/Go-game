using Go.Backend.Application.Interfaces;
using Go.Backend.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Go.Backend.Infrastructure.Persistence
{
    public class GameRepository : IGameRepository
    {
        private readonly GoDbContext _context;

        public GameRepository(GoDbContext context)
        {
            _context = context;
        }

        public async Task<GameMatch> CreateAsync(GameMatch game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<GameMatch?> GetByIdAsync(Guid id)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task SaveAsync(GameMatch game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }
    }
}