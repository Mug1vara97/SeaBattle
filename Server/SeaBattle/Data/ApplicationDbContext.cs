using Microsoft.EntityFrameworkCore;
using SeaBattle.Models;

namespace SeaBattle.Data
{
    /// <summary>
    /// Контекст базы данных приложения SeaBattle.
    /// Обеспечивает доступ к таблицам и управление данными игры.
    /// </summary>
    /// <remarks>
    /// Контекст включает следующие сущности:
    /// <list type="bullet">
    /// <item><description>Users - информация о пользователях</description></item>
    /// <item><description>GameHistories - история игр</description></item>
    /// <item><description>PlayerRankings - рейтинги игроков</description></item>
    /// </list>
    /// 
    /// Пример использования:
    /// <code>
    /// using (var context = new ApplicationDbContext(options))
    /// {
    ///     // Получение пользователя
    ///     var user = await context.Users.FindAsync(userId);
    ///     
    ///     // Получение истории игр
    ///     var history = await context.GameHistories
    ///         .Where(h => h.PlayerUsername == username)
    ///         .ToListAsync();
    /// }
    /// </code>
    /// </remarks>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр контекста базы данных.
        /// </summary>
        /// <param name="options">Параметры конфигурации контекста</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Получает или устанавливает набор пользователей в базе данных.
        /// </summary>
        /// <remarks>
        /// Содержит основную информацию о зарегистрированных пользователях,
        /// включая учетные данные и настройки профиля.
        /// </remarks>
        public virtual DbSet<User> Users { get; set; }

        /// <summary>
        /// Получает или устанавливает набор записей истории игр.
        /// </summary>
        /// <remarks>
        /// Хранит информацию о завершенных играх, включая:
        /// <list type="bullet">
        /// <item><description>Участников игры</description></item>
        /// <item><description>Результат игры</description></item>
        /// <item><description>Время завершения</description></item>
        /// </list>
        /// </remarks>
        public virtual DbSet<GameHistory> GameHistories { get; set; }

        /// <summary>
        /// Получает или устанавливает набор рейтингов игроков.
        /// </summary>
        /// <remarks>
        /// Содержит статистику игроков:
        /// <list type="bullet">
        /// <item><description>Текущий рейтинг</description></item>
        /// <item><description>Количество побед и поражений</description></item>
        /// <item><description>Общее количество игр</description></item>
        /// </list>
        /// </remarks>
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