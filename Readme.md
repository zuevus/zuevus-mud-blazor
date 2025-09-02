# ZuevUS.Mud - Blazor Portfolio Project

## 📋 О проекте

**ZuevUS.Mud** - это презентационное веб-приложение, разработанное на современном стеке технологий с использованием Blazor WebAssembly, MudBlazor UI компонентов и gRPC сервисов. Проект демонстрирует лучшие практики разработки современных веб-приложений.

**🎯 Цель проекта**: Создать полнофункциональное портфолио-приложение, демонстрирующее навыки работы с современными технологиями .NET экосистемы.

**🌐 Живое приложение**: [https://mudblazor-example.zuevus.ru](https://mudblazor-example.zuevus.ru)

## 🛠 Технологический стек

### Frontend
- **Blazor WebAssembly** - SPA фреймворк на .NET
- **MudBlazor** - Material Design компонентная библиотека
- **OIDC Authentication** - Аутентификация через Azure AD
- **gRPC-Web** - Высокопроизводительная коммуникация с сервером

### Backend
- **ASP.NET Core** - Веб-фреймворк
- **gRPC Services** - Высокопроизводительные микросервисы
- **Entity Framework Core** - ORM для работы с базой данных
- **SQLite** - Встраиваемая база данных
- **Serilog** - Структурированное логирование

### Инфраструктура
- **Docker** - Контейнеризация приложения
- **NUnit** - Фреймворк для unit-тестирования
- **Moq** - Mocking framework для тестов

## 🗄️ Архитектура базы данных

### Основные таблицы

#### Таблица: `Orders`
```sql
CREATE TABLE Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    OrderType TEXT NOT NULL,
    Status TEXT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    Deadline DATETIME NOT NULL,
    ClientName TEXT NOT NULL,
    ClientEmail TEXT NOT NULL,
    CreatedByUserId TEXT NOT NULL
);
```


#### Таблица: `UserProfiles`
```sql
CREATE TABLE UserProfiles (
    UserId TEXT PRIMARY KEY,
    UserName TEXT NOT NULL,
    Email TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedDate DATETIME NOT NULL,
    LastLoginDate DATETIME NULL
);
```

### Перечисления (Enums)
#### OrderType
   - **WebsiteDevelopment** - Разработка сайта
   - **MobileApp** - Мобильное приложение
   - **ApiDevelopment** - Разработка API
   - DatabaseDesign - Дизайн базы данных
   - SystemMaintenance - Обслуживание системы

#### BugFixing - Исправление ошибок
   - Consultation - Консультация
   - OrderStatus
   - New - Новый
   - InProgress - В процессе
   - Completed - Завершен
   - Cancelled - Отменен

#### UserRole
   - Admin - Администратор
   - User - Пользователь
    
## 📁 Структура проекта

```text
ZuevUS.Mud/
├── ZuevUS.Mud.Client/          # Blazor WebAssembly фронтенд
├── ZuevUS.Mud.Server/          # ASP.NET Core бэкенд
├── ZuevUS.Mud.Database/        # Модели и контекст БД
├── ZuevUS.Mud.Services/        # gRPC сервисы
├── ZuevUS.Mud.Services.Tests/  # Unit тесты
└── docker-compose.yml          # Docker конфигурация
```

🌟 Ключевые особенности
 - Современная архитектура - Clean Architecture с разделением ответственности
 - Высокая производительность - gRPC для межсервисного взаимодействия
 - Безопасность - OIDC аутентификация и ролевая авторизация
 - Масштабируемость - Микросервисная архитектура
 - Надежность - Comprehensive тестирование и логирование
 - Производительность - Blazor WebAssembly для клиентской стороны

## 📈 Статус проекта
###    ✅ Завершено
        Базовая архитектура
        gRPC сервисы
        База данных

### 🔄 В процессе
   - Аутентификация
   - Основной UI
   - Мониторинг и analytics
   - Дополнительные функции
        - Мониторинг
        - Kafka брокер для отправки сообщени
        - Сервис для приёма и пересылки сообщений в телеграм/макс каналы
       
### Для релиза
```bash
git tag release_v1.0.0
git push release_v1.0.0
```
## 👨‍💻 Разработчик
Юрий Зуев - Fullstack .NET разработчик

Email: zuevus@gmail.com
GitHub: github.com/zuevus

Website: zuevus.ru

## 📄 Лицензия
Этот проект лицензирован под MIT License - смотрите файл LICENSE для деталей.