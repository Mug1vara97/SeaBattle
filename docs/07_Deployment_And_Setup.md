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

- PostgreSQL: пользователь - postgres, пароль - 1000-7, база данных - seabattle, порт - 5432

Дальше необходимо перейти в папку Backend/TekkenStats (команды вводятся по очереди из корня проекта)

> $ cd Server

> $ dotnet run --project SeaBattle

> $ cd client (или открыть еще один терминал в папке Frontend/tekken-stats)

> $ npm i

> $ npm run dev

