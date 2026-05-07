# WeatherStation API

Метеорологічна API для збору та звітування даних з метеостанцій.

## 🏗️ Архітектура

```
WeatherStation.Api         - ASP.NET Core Web API
├── Models                 - Domain models (Station, Reading, Alert)
├── Controllers            - API endpoints
├── Services              - Business logic (ReadingService)
├── Data                  - EF Core DbContext, migrations, seed data
└── Dtos                  - Request/Response models

WeatherStation.UnitTests        - xUnit unit tests
WeatherStation.IntegrationTests - xUnit integration tests (WebApplicationFactory)
WeatherStation.DbTests         - xUnit database tests (Testcontainers)
WeatherStation.PerformanceTests - k6 performance tests
```

## 📋 Вимоги

- .NET 10.0+
- PostgreSQL 16+ (для production)
- Docker (опціонально)
- k6 (для performance тестів)

## 🚀 Запуск локально

### 1. Налаштування бази даних

```bash
# Встановіть PostgreSQL локально або запустіть у Docker
docker run -d \
  --name postgres \
  -e POSTGRES_DB=weatherstation \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine
```

### 2. Наповнення бази даних

```bash
dotnet run --project WeatherStation.Api -- --seed
```

Це створить 35 станцій з реалістичними даними (усього > 10 000 записів).

### 3. Запуск API

```bash
dotnet run --project WeatherStation.Api
```

API буде доступна на `http://localhost:5000`.

### 4. Перевірка здоров'я

```bash
curl http://localhost:5000/health
```

## 🧪 Тестування

### Unit-тести

```bash
dotnet test WeatherStation.UnitTests
```

- Генерація alert за порогами
- Валідація діапазонів значень
- Розрахунок середнього

### Інтеграційні тести

```bash
dotnet test WeatherStation.IntegrationTests
```

- WebApplicationFactory
- Подача вимірювання зі створенням alert
- Endpoint середніх значень
- Фільтрація readings за датами

### DB-тести

```bash
dotnet test WeatherStation.DbTests
```

Вимагає Docker для Testcontainers PostgreSQL.

### Performance-тести (k6)

Запустіть API:

```bash
dotnet run --project WeatherStation.Api -- --seed
```

Потім у іншому терміналі запустіть k6:

```bash
# Тест високочастотних подань вимірювань
k6 run WeatherStation.PerformanceTests/k6/high_frequency_readings.js

# Тест навантаження для average endpoint
k6 run WeatherStation.PerformanceTests/k6/average_load.js
```

## 📡 API Ендпоінти

### Станції

- `GET /api/stations` - Отримати всі станції
- `POST /api/stations` - Зареєструвати нову станцію

### Вимірювання

- `POST /api/stations/{id}/readings` - Подати вимірювання
- `GET /api/stations/{id}/readings` - Отримати вимірювання (фільтр за датами)
- `GET /api/stations/{id}/latest` - Отримати останнє вимірювання
- `GET /api/stations/{id}/average?from=&to=` - Отримати середні значення

### Сповіщення

- `GET /api/alerts` - Отримати активні непідтверджені сповіщення
- `PATCH /api/alerts/{id}/acknowledge` - Підтвердити сповіщення

## 🐳 Docker

### Побудова образу

```bash
docker build -t weatherstation-api .
```

### Запуск контейнера

```bash
docker run -d \
  --name weatherstation \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Database=weatherstation;Username=postgres;Password=postgres" \
  -p 5000:5000 \
  weatherstation-api
```

### Docker Compose

```bash
docker-compose up
```

## 🔄 GitHub Actions CI/CD

Наступні workflows запускаються автоматично на push/PR:

### [ci.yml](.github/workflows/ci.yml)
- Build
- Unit Tests
- Integration Tests
- Database Tests
- Code Coverage

### [k6.yml](.github/workflows/k6.yml)
- Performance testing
- High-frequency load
- Average computation stress test
- Daily scheduled runs

### [code-quality.yml](.github/workflows/code-quality.yml)
- Static analysis (Roslynator)
- Code formatting checks

### [security.yml](.github/workflows/security.yml)
- Outdated packages check
- Dependency vulnerabilities (dotnet-audit)
- Trivy filesystem scanning

### [docker-build.yml](.github/workflows/docker-build.yml)
- Build and push Docker образу на ghcr.io

### [release.yml](.github/workflows/release.yml)
- Автоматичний release при тегах `v*`

## 📊 Бізнес-правила

- Температура: -60..60°C
- Вологість: 0-100%
- Вітер: 0+ км/год
- Сповіщення:
  - `HighTemp`: T > 40°C
  - `LowTemp`: T < -30°C (резерв)
  - `HighWind`: V > 100 км/год
- Вимірювання мають бути хронологічними для кожної станції
- Неактивні станції не можуть подавати вимірювання

## 📦 Залежності

- ASP.NET Core 10.0
- Entity Framework Core 10.0
- Npgsql 10.0
- xUnit 2.9
- AutoFixture 4.18
- Microsoft.AspNetCore.Mvc.Testing
- DotNet.Testcontainers
- k6 (для performance тестів)

## 📝 Ліцензія

Навчальний проект

## 👨‍💼 Контакти

Для питань та пропозицій створіть Issue або Pull Request.
