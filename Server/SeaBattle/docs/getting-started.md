# Начало работы с SeaBattle

## Требования к системе

Для работы с проектом необходимо:

- .NET 8.0 SDK
- PostgreSQL 15
- Docker (опционально)
- Visual Studio 2022 или VS Code с C# расширением

## Установка и настройка

1. Клонируйте репозиторий:
```bash
git clone https://github.com/Mug1vara97/SeaBattle
cd SeaBattle/Server
```

2. Восстановите зависимости:
```bash
dotnet restore
```

3. Настройте строку подключения к базе данных в `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=seabattle;Username=your_username;Password=your_password"
  }
}
```

4. Примените миграции базы данных:
```bash
dotnet ef database update
```

5. Запустите приложение:
```bash
dotnet run
```

## Запуск через Docker

1. Соберите Docker образ:
```bash
docker build -t seabattle .
```

2. Запустите контейнер:
```bash
docker run -p 5000:80 seabattle
```

## Документация API

Документация API доступна через Swagger UI после запуска приложения по адресу:
```
http://localhost:5000/swagger
```

## Структура решения

- `SeaBattle/` - основной проект
  - `Controllers/` - REST API контроллеры
  - `Models/` - модели данных с XML-документацией
  - `Services/` - бизнес-логика
  - `Hubs/` - SignalR хабы
  - `Data/` - работа с базой данных
- `GameServiceTests/` - модульные тесты

## XML-документация

Все модели в проекте содержат подробную XML-документацию на русском языке. Документация включает:
- Описание назначения классов
- Описание свойств
- Описание методов и их параметров
- Примеры использования (где применимо)