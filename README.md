
# SteelWarehouse

## Описание проекта

**SteelWarehouse** — это веб-сервис для управления складом стальных рулонов. API позволяет добавлять, удалять, просматривать и фильтровать рулоны, а также получать статистику по складу за определённый период

## Стек технологий

-   **.NET 8 & ASP.NET Core**
-   **Dapper**
-   **DbUp**
-   **PostgreSQL**
- **xUnit & Moq**
-   **Docker**

## Архитектурные особенности

### Подход Database First

Проект использует подход **Database First**. Структура базы данных не генерируется из C# моделей (как в EF Core Code First), а управляется явно через SQL-миграции с помощью **DbUp**
    
### Модульность и Расширяемость

Слой доступа к данным скрыт за абстракцией — интерфейсом `ISteelRollRepository`

Это делает систему **модульной**: можно легко заменить PostgreSQL на любое другое хранилище. Для этого достаточно:

1.  Создать новый класс
2.  Реализовать интерфейс `ISteelRollRepository`

Текущая реализация уже включает два репозитория:

-   `SteelRollRepository` (PostgreSQL)
-   `InMemorySteelRollRepository` (Хранение в оперативной памяти)


## Конфигурация

Приложение поддерживает два типа хранилища данных: `InMemory` и `Postgres`. Тип хранилища и строка подключения к базе данных настраиваются через файл `appsettings.json` или через переменные окружения (ENV). Переменные окружения имеют приоритет

### Настройка через `appsettings.json`

```json
{
  "ConnectionStrings": {
	"Default": "Host=localhost;Port=5432;Database=steelwarehousedb;Username=user;Password=password"
  },
  "Storage": {
    "Type": "InMemory" // Или "Postgres"
  },
}
```

### Настройка через переменные окружения (ENV)

Для переопределения настроек в Docker используются переменные окружения 

-   `Storage__Type`: Устанавливает тип хранилища (`Postgres` или `InMemory`)
-   `ConnectionStrings__Default`: Устанавливает строку подключения к базе данных

Пример для `docker-compose.yml`:
```yaml
environment:
  - Storage__Type=Postgres
  - ConnectionStrings__Default=Host=db;...
```

## Запуск проекта с помощью Docker

Для запуска проекта убедитесь, что у вас установлен Docker и Docker Compose.

1.  Склонируйте репозиторий
2.  В корневой директории проекта выполните команду:
    ```bash
    docker-compose up --build
    ```
3.  Swagger UI для тестирования API будет доступен по адресу `http://localhost:8080/swagger`