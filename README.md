# Notification Service

A .NET 8 ASP.NET Core Web API that sends **Firebase Cloud Messaging (FCM) push notifications** and **SMTP transactional emails** using the **Outbox Pattern** with PostgreSQL and background workers.

## Features

- Register and deactivate device tokens per user (with locale support)
- Queue **broadcast** or **targeted** push notifications with localized EN/AR content
- Send transactional emails (email confirmation, password reset, 2FA) via SMTP
- Background workers process queued items (outbox) with retries
- Delivery tracking, per-device details, and status endpoints
- Invalid-token cleanup (deactivated automatically when FCM rejects them)
- Stale-lock recovery so crashed workers don't block batches
- Swagger UI in development/staging

## Tech Stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8 + PostgreSQL 16
- Firebase Cloud Messaging HTTP v1 API (FirebaseAdmin + Google OAuth)
- MailKit (SMTP)
- FluentValidation validator definitions, with direct checks in the current use cases
- Background workers via `BackgroundService`
- Docker + Docker Compose

## Project Structure

```
NotificationService.Api/             # Presentation layer and composition root
├── Controllers/                     # HTTP endpoints
├── Filters/                         # API exception filters
└── Program.cs                       # Configuration, Swagger, DI, and startup

NotificationService.Application/     # Application layer
├── DTOs/                            # Request and response contracts
├── FluentValidation/                # Validator definitions
├── Interfaces/                      # Repository, sender, and processor contracts
└── UseCases/                        # Operations called by controllers

NotificationService.Core/            # Domain layer
├── Entities/                        # Tokens, outbox records, and deliveries
├── Enums/                           # Statuses, platforms, and message types
└── ValueObjects/                    # Localized notification content

NotificationService.Infrastructure/  # Infrastructure implementations
├── Data/                            # EF Core DbContext and configurations
├── Migrations/                      # EF Core database migrations
├── Repositories/                    # PostgreSQL repository implementations
├── Services/                        # FCM and SMTP integrations
├── Processors/                      # FCM and SMTP outbox processors
└── Workers/                         # Background outbox workers
```

Controllers depend on application use cases. Use cases write pending records through application interfaces. Infrastructure implements those interfaces and runs the FCM and SMTP processors through background workers. Fan-out, payload serialization, token resolution, retries, and delivery tracking are handled outside the HTTP controllers.

## Prerequisites

- .NET SDK 8.0+
- PostgreSQL 16+ (if running without Docker)
- Docker + Docker Compose (optional)
- Firebase service account credentials with FCM permissions
- SMTP account (e.g. Gmail app password) for email sending

## Configuration

The service reads configuration from environment variables (`.env` at repo root, or container env vars).

```env
ASPNETCORE_ENVIRONMENT=Development

# Database (used by the API)
ConnectionStrings__DefaultConnection=Host=localhost;Port=5435;Database=NotificationsDB1;Username=postgres;Password=postgres

# Firebase Cloud Messaging (service account)
Firebase__ProjectId=your-project-id
Firebase__ClientEmail=xxx@your-project.iam.gserviceaccount.com
Firebase__PrivateKey="-----BEGIN PRIVATE KEY-----\\n...\\n-----END PRIVATE KEY-----\\n"
Firebase__TokenUri=https://oauth2.googleapis.com/token

# SMTP (MailKit)
Smtp__Host=smtp.gmail.com
Smtp__Port=465
Smtp__UseSsl=true
Smtp__Username=you@gmail.com
Smtp__Password=app-password
Smtp__SenderName=Notifications
Smtp__SenderEmail=you@gmail.com
Smtp__BaseUrl=https://yourapp.com

# Docker Postgres container only
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=NotificationsDB1
```

> `Firebase__PrivateKey` must preserve newlines — use escaped `\\n` exactly as provided by Google. The `.env` file is gitignored; never commit it.

## Running with Docker

1. Fill in `.env` with the values above.
2. Build and start services:

```bash
docker compose up --build
```

3. Apply database migrations:

```bash
dotnet ef database update --project NotificationService.Infrastructure --startup-project NotificationService.Api
```

4. API is exposed at `http://localhost:5000`, PostgreSQL at `localhost:5435`.

> `docker-compose.yaml` currently forwards only the database + Firebase variables to the API container. If you use SMTP in Docker, add matching `Smtp__*` lines under `api.environment`.

## Running Locally (without Docker)

```bash
# 1. Ensure PostgreSQL is running and set ConnectionStrings__DefaultConnection
# 2. Set Firebase + SMTP env vars (or .env file)
# 3. Apply migrations
dotnet ef database update --project NotificationService.Infrastructure --startup-project NotificationService.Api
# 4. Run
dotnet run --project NotificationService.Api
```

Default local URL (from launch settings): `http://localhost:5044`.

## Swagger

- `http://localhost:5044/` (local)
- `http://localhost:5000/` (Docker)

## API Endpoints

Base route: `/api`

### Device Tokens

`POST /api/device-tokens/register`

```json
{
  "userId": "user-123",
  "token": "fcm-device-token",
  "platform": "Android",
  "locale": "en"
}
```

`platform`: `Android`, `IOS`, `Web`. `locale`: `en` or `ar` (defaults to `en`).

`DELETE /api/device-tokens/deactivate`

```json
{ "token": "fcm-device-token" }
```

`DELETE /api/device-tokens/deactivate/user/{userId}`

### Notifications

`POST /api/notifications/broadcast` — one outbox row, sent to all active devices.

```json
{
  "title": { "en": "System Update", "ar": "تحديث النظام" },
  "body": { "en": "Maintenance at 10 PM", "ar": "صيانة الساعة 10 مساءً" },
  "data": { "type": "maintenance", "priority": "high" },
  "scheduledAt": "2026-03-03T18:00:00Z"
}
```

`POST /api/notifications/send` — one outbox row **per user** (fan-out), sent to each user's active devices.

```json
{
  "userIds": ["user-123", "user-456"],
  "title": { "en": "Hello", "ar": "مرحبا" },
  "body": { "en": "You have a new message", "ar": "لديك رسالة جديدة" },
  "data": { "screen": "inbox" },
  "scheduledAt": "2026-03-03T18:00:00Z"
}
```

Both return `202 Accepted` with the outbox id, status and queued count (`QueuedCount` = number of outbox rows created).

`GET /api/notifications/{notificationId}/status` — aggregate status + per-delivery details.

`GET /api/notifications/user/{userId}/history?locale=en` — user's notifications resolved in the given locale (`en`/`ar`).

### Email (SMTP)

`POST /api/smtp/send-confirm-email` — confirmation link email

`POST /api/smtp/send-reset-password` — password reset email

`POST /api/smtp/send-2FA` — two-factor code email

All three take:

```json
{
  "toEmail": "user@example.com",
  "token": "abc123",
  "scheduledAt": "2026-03-03T18:00:00Z"
}
```

and return `202 Accepted` with the outbox email id and status (`Pending`).

Email type is stored on the outbox row; the SMTP worker picks the right template/payload per type (`ConfirmEmail`, `ResetPassword`, `TwoFactorCode`).

## Validation

The current enqueue and token-registration use cases perform their request checks directly in the application layer. The project also contains FluentValidation validator classes in `NotificationService.Application/FluentValidation`, and the API registers them during startup, but the use cases do not depend on `IValidator`.

Key rules:

- Device token registration: `userId` and `token` required (token <= 512 chars), `platform` must be a defined `Android`/`IOS`/`Web` value.
- Broadcast/send: at least one of `title` or `body` required; `scheduledAt` must be in the future when provided.
- Send to users: `userIds` required, non-empty, max 1000 per request.
- Email: `toEmail` required and valid (<= 200), `token` required (<= 512), `scheduledAt` in the future when provided.

## How Processing Works (Outbox Pattern)

Controllers only **enqueue**: use cases check the request, write one or more outbox rows (`Pending`) and return `202` — no actual sending happens on the request path.

Two background workers consume the outbox tables every 5 seconds:

1. `FCMWorker` → `OutboxNotifications`, `SmtpWorker` → `OutboxEmails`
2. Each iteration claims a batch of up to **50** rows inside a transaction (`Processing` + `LockedBy`), so multiple instances don't double-send.
3. Rows stuck in `Processing` longer than **5 minutes** are released (stale-lock recovery).
4. For each item:
   - FCM: resolves target devices (broadcast = all active tokens; targeted = per user), resolves the title/body against the token's locale, and sends via FCM. Each result is stored in `NotificationDeliveries`. Tokens FCM rejects as `UNREGISTERED`/`INVALID_ARGUMENT` are auto-deactivated.
   - SMTP: `SendByTypeAsync` deserializes the stored payload and sends the matching email. Each result goes to `EmailDeliveries`.
5. Success → item marked `Sent`/processed. Failure → `Failed` delivery row + retry counter incremented, re-queued as `Pending` until **`MaxRetries` (3)**, then marked `Failed`.

## Database

Main tables:

- `DeviceTokens`
- `OutboxNotifications` + `NotificationDeliveries`
- `OutboxEmails` + `EmailDeliveries`

Migrations are included in `NotificationService.Infrastructure/Migrations`.

## Troubleshooting

- `400 Bad Request` with a validation array: check the rules above (title/body, `scheduledAt`, `userIds`, email format).
- FCM auth errors: verify `Firebase__ProjectId`, `Firebase__ClientEmail`, `Firebase__PrivateKey` (with real `\n`), `Firebase__TokenUri`.
- No push received: confirm device tokens are registered, active, and FCM approves them (invalid tokens are deactivated automatically).
- Emails not sending: check `Smtp__*` values (host/port/SSL/app password) and that the SMTP container env vars are forwarded.
- DB connection errors: validate `ConnectionStrings__DefaultConnection` and the PostgreSQL port (`5435` in Docker).