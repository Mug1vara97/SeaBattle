using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace SeaBattle.Data
{
    /// <summary>
    /// Фабрика для создания контекста базы данных во время разработки.
    /// Используется инструментами Entity Framework Core для миграций и обновления базы данных.
    /// </summary>
    /// <remarks>
    /// Фабрика обеспечивает:
    /// <list type="bullet">
    /// <item><description>Создание контекста БД вне среды выполнения приложения</description></item>
    /// <item><description>Загрузку конфигурации подключения к БД из appsettings.json</description></item>
    /// <item><description>Поддержку инструментов командной строки EF Core</description></item>
    /// </list>
    /// 
    /// Используется при выполнении команд:
    /// <code>
    /// dotnet ef migrations add InitialCreate
    /// dotnet ef database update
    /// </code>
    /// </remarks>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Создает новый экземпляр контекста базы данных для использования во время разработки.
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются)</param>
        /// <returns>Настроенный экземпляр контекста базы данных</returns>
        /// <remarks>
        /// Метод выполняет:
        /// <list type="number">
        /// <item><description>Определение текущей директории проекта</description></item>
        /// <item><description>Загрузку конфигурации из appsettings.json</description></item>
        /// <item><description>Настройку подключения к базе данных</description></item>
        /// <item><description>Создание и возврат контекста с настроенными параметрами</description></item>
        /// </list>
        /// </remarks>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в конфигурации.");
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 