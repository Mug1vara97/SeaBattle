---
_layout: landing
---

Добро пожаловать в документацию проекта SeaBattle - многопользовательской игры в морской бой!

## Быстрая навигация

<div class="card-deck">
  <div class="card">
    <div class="card-body">
      <h5 class="card-title">Начало работы</h5>
      <p class="card-text">Узнайте больше о проекте и его возможностях.</p>
      <a href="docs/getting-started.html" class="btn btn-primary">Начать</a>
    </div>
  </div>
  <div class="card">
    <div class="card-body">
      <h5 class="card-title">Игровой процесс</h5>
      <p class="card-text">Изучите правила игры и механики.</p>
      <a href="docs/gameplay.html" class="btn btn-primary">Изучить</a>
    </div>
  </div>
  <div class="card">
    <div class="card-body">
      <h5 class="card-title">API</h5>
      <p class="card-text">Техническая документация по API проекта.</p>
      <a href="api/index.html" class="btn btn-primary">Документация</a>
    </div>
  </div>
</div>

## Основные разделы

### Модели
Основные модели данных, используемые в проекте:
- [Game](api/SeaBattle.Models.Game.yml) - игровая сессия
- [User](api/SeaBattle.Models.User.yml) - пользователь
- [PlayerRanking](api/SeaBattle.Models.PlayerRanking.yml) - рейтинг игрока

### Сервисы
Основная бизнес-логика:
- [GameService](api/SeaBattle.Services.GameService.yml) - управление играми
- [UserService](api/SeaBattle.Services.UserService.yml) - управление пользователями

### API
REST API и real-time взаимодействие:
- [GameController](api/SeaBattle.Controllers.GameController.yml) - игровые операции
- [GameHub](api/SeaBattle.Hubs.GameHub.yml) - real-time обновления

### Данные
Работа с базой данных:
- [ApplicationDbContext](api/SeaBattle.Data.ApplicationDbContext.yml) - контекст БД

## Дополнительная информация

- [Исходный код на GitHub](https://github.com/yourusername/SeaBattle)
- [Отчеты об ошибках](https://github.com/yourusername/SeaBattle/issues)
- [Руководство по контрибьютингу](docs/contributing.html) 