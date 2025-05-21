using Microsoft.EntityFrameworkCore;
using SeaBattle.Models;

namespace SeaBattle.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<GameHistory> GameHistories { get; set; }
        public virtual DbSet<PlayerRanking> PlayerRankings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<GameHistory>()
                .HasIndex(h => h.PlayerUsername);
            modelBuilder.Entity<GameHistory>()
                .HasIndex(h => h.GameFinishedAt);

            modelBuilder.Entity<PlayerRanking>()
                .HasKey(r => r.PlayerUsername);

            modelBuilder.Entity<PlayerRanking>()
                .HasIndex(r => r.Rating);
        }
    }
} 