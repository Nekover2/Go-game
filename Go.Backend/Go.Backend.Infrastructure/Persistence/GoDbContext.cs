using Microsoft.EntityFrameworkCore;
using Go.Backend.Application.Models;
using Go.Backend.Domain.Entities;
using Newtonsoft.Json;

namespace Go.Backend.Infrastructure.Persistence
{
    public class GoDbContext : DbContext
    {
        public DbSet<GameMatch> Games { get; set; }

        public GoDbContext(DbContextOptions<GoDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Cấu hình dùng SQLite nếu chưa được inject (dùng cho lúc chạy migration)
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=gogame.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình lưu trữ cho GameMatch
            modelBuilder.Entity<GameMatch>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Chuyển đổi Board <-> JSON String khi lưu vào DB
                entity.Property(e => e.Board)
                    .HasConversion(
                        boardObj => JsonConvert.SerializeObject(boardObj), // Lưu vào DB
                        jsonStr => JsonConvert.DeserializeObject<Board>(jsonStr) ?? new Board(19) // Đọc ra
                    );
            });
        }
    }
}