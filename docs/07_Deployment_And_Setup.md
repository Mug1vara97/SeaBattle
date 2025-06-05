# 7. Развертывание и настройка (с использованием Docker)

## Если есть docker

Ввести команду в корне репозитория:

> $ docker-compose up --build -d

После этого можно будет получить доступ к приложению на localhost:8081

## Если docker нет

Необходимо установить и настроить следующие компоненты:

- [PostgreSQL](https://www.postgresql.org/download/)
- [Node.js](https://nodejs.org/en/download)
- [.NET 8](https://dotnet.microsoft.com/en-us/download)

По умолчанию в appSettings.development.json (в каждом проекте свои настройки) содержатся строки подключения со следующими данными (при необходимости исправить или прокинуть свои):

- PostgreSQL: пользователь - postgres, пароль - 1000-7, база данных - seabattle, порт - 5433

Дальше необходимо перейти в папку SeaBattle (команды вводятся по очереди из корня проекта)

> $ cd Server

> $ dotnet run --project SeaBattle

> $ cd client (или открыть еще один терминал в папке Frontend/tekken-stats)

> $ npm i

> $ npm run dev

## Использование Docker Hub

Для запуска приложения с использованием образов из Docker Hub необходимо:

1. Загрузить необходимые образы:

> docker pull mug1vara/seabattle:client-latest

> docker pull mug1vara/seabattle:server-latest

> docker pull mug1vara/seabattle:db-latest

2. Создать файл docker-compose.yml со следующим содержимым:

```yaml
services:
  db:
    image: mug1vara/seabattle:db-latest
    container_name: seabattle-postgres
    environment:
      POSTGRES_USER: seabattleuser   
      POSTGRES_PASSWORD: seabattlepass 
      POSTGRES_DB: seabattledb 
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5433:5432"
    restart: unless-stopped

  server:
    image: mug1vara/seabattle:server-latest
    ports:
      - "5183:8080"  
    container_name: seabattle-server
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=seabattledb;Username=seabattleuser;Password=seabattlepass;"
    depends_on:
      - db 
    restart: unless-stopped

  client:
    image: mug1vara/seabattle:client-latest
    ports:
      - "3000:80" 
    container_name: seabattle-client
    depends_on:
      - server
    restart: unless-stopped

volumes:
  postgres_data: 
```

3. Запустить контейнеры:

> docker-compose up -d


