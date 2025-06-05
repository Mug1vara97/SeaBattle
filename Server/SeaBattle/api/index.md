# API Documentation

## Основные разделы

### Модели
- [Game](SeaBattle.Models.Game.yml) - основная модель игровой сессии
- [Position](SeaBattle.Models.Position.yml) - координаты на игровом поле
- [CellState](SeaBattle.Models.CellState.yml) - состояния клеток

### Сервисы
- [IGameService](SeaBattle.Services.IGameService.yml) - интерфейс игрового сервиса
- [GameService](SeaBattle.Services.GameService.yml) - реализация игрового сервиса
- [IUserService](SeaBattle.Services.IUserService.yml) - интерфейс сервиса пользователей
- [UserService](SeaBattle.Services.UserService.yml) - реализация сервиса пользователей

### Контроллеры
- [GameController](SeaBattle.Controllers.GameController.yml) - управление игровым процессом
- [AuthController](SeaBattle.Controllers.AuthController.yml) - аутентификация и авторизация

### Real-time взаимодействие
- [GameHub](SeaBattle.Hubs.GameHub.yml) - хаб для игрового процесса
- [LobbyHub](SeaBattle.Hubs.LobbyHub.yml) - хаб для лобби

### Работа с данными
- [ApplicationDbContext](SeaBattle.Data.ApplicationDbContext.yml) - контекст базы данных
- [DesignTimeDbContextFactory](SeaBattle.Data.DesignTimeDbContextFactory.yml) - фабрика контекста 