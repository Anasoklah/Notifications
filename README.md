# Notification Service

A .NET 8 Web API for sending Firebase Cloud Messaging (FCM) push notifications using the outbox pattern and PostgreSQL.

## Features

- Register and deactivate device tokens per user
- Queue broadcast or targeted notifications
- Background worker processes queued notifications
- Delivery tracking and notification status endpoint
- Retry support with stale-lock recovery
- Swagger UI in development

## Tech Stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8
- PostgreSQL 16
- Firebase Cloud Messaging HTTP v1 API
- FluentValidation
- Docker + Docker Compose

## Project Structure

- `NotificationService.Api` - API endpoints, DI configuration, app startup
- `NotificationService.Core` - DTOs, entities, enums, interfaces, validators
- `NotificationService.Infrastructure` - EF Core context/configurations, repository, FCM sender, outbox worker

## Prerequisites

- .NET SDK 8.0+
- PostgreSQL 16+ (if running without Docker)
- Docker + Docker Compose (optional, for containerized run)
- Firebase service account credentials with FCM permissions

## Configuration

The service reads configuration from environment variables.

Required variables:

- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__DefaultConnection`
- `Firebase__ProjectId`
- `Firebase__ClientEmail`
- `Firebase__PrivateKey`
- `POSTGRES_USER` (for Docker DB)
- `POSTGRES_PASSWORD` (for Docker DB)
- `POSTGRES_DB` (for Docker DB)

### Notes on Firebase Private Key

`Firebase__PrivateKey` must preserve newlines. In `.env`, use escaped newlines (`\\n`) exactly as provided by Google.

Example:

```env
Firebase__PrivateKey="-----BEGIN PRIVATE KEY-----\\n...\\n-----END PRIVATE KEY-----\\n"
```

## Running with Docker (Recommended)

1. Fill in `.env` with PostgreSQL and Firebase values.
2. Build and start services:

```bash
docker compose up --build
```

3. Apply database migrations (first run or after schema changes):

```bash
dotnet ef database update --project NotificationService.Infrastructure --startup-project NotificationService.Api
```

4. API will be available at:

- `http://localhost:5000`

PostgreSQL is exposed on:

- `localhost:5433`

## Running Locally (without Docker)

1. Ensure PostgreSQL is running and update:

- `ConnectionStrings__DefaultConnection`

2. Set Firebase environment variables.
3. Apply EF Core migrations:

```bash
dotnet ef database update --project NotificationService.Infrastructure --startup-project NotificationService.Api
```

4. Run the API:

```bash
dotnet run --project NotificationService.Api
```

Default local URL (from launch settings):

- `http://localhost:5044`

## Swagger

In development, Swagger UI is served at app root:

- `http://localhost:5044/` (local)
- `http://localhost:5000/` (Docker)

## API Endpoints

Base route: `/api`

### Device Tokens

1. Register token

`POST /api/device-tokens/register`

```json
{
  "userId": "user-123",
  "token": "fcm-device-token",
  "platform": "Android"
}
```

`platform` values: `Android`, `IOS`, `Web`

2. Deactivate one token

`DELETE /api/device-tokens/deactivate`

```json
{
  "token": "fcm-device-token"
}
```

3. Deactivate all tokens for a user

`DELETE /api/device-tokens/deactivate/user/{userId}`

### Notifications

1. Broadcast notification

`POST /api/notifications/broadcast`

```json
{
  "title": "System Update",
  "body": "Maintenance at 10 PM",
  "data": {
    "type": "maintenance",
    "priority": "high"
  },
  "scheduledAt": "2026-03-03T18:00:00Z"
}
```

2. Send to selected users

`POST /api/notifications/send`

```json
{
  "userIds": ["user-123", "user-456"],
  "title": "Hello",
  "body": "You have a new message",
  "data": {
    "screen": "inbox"
  },
  "scheduledAt": "2026-03-03T18:00:00Z"
}
```

3. Get notification status

`GET /api/notifications/{notificationId}/status`

## Validation Rules

- Device token registration:
  - `userId` is required
  - `token` is required and max 512 chars
  - `platform` must be a valid enum value
- Broadcast/send:
  - at least one of `title` or `body` is required
  - `scheduledAt` must be in the future if provided
- Send to users:
  - `userIds` required
  - up to 1000 users per request

## How Processing Works (Outbox Pattern)

1. API writes notifications to `OutboxNotifications` with status `Pending`.
2. `OutboxWorker` runs every 5 seconds.
3. Worker locks a batch (`Processing`) to avoid duplicate processing.
4. Worker resolves target tokens, sends via FCM, and stores rows in `NotificationDeliveries`.
5. Notification is marked `Sent` when processed, or retried until `MaxRetries`.

## Database

Main tables:

- `DeviceTokens`
- `OutboxNotifications`
- `NotificationDeliveries`

Migrations are already included in `NotificationService.Infrastructure/Migrations`.

## Troubleshooting

- `400 Bad Request` on send/broadcast: check validator rules (`title/body`, `scheduledAt`, `userIds`).
- FCM auth errors: verify `Firebase__ProjectId`, `Firebase__ClientEmail`, and `Firebase__PrivateKey`.
- No deliveries created: confirm device tokens are registered and active.
- DB connection errors: validate `ConnectionStrings__DefaultConnection` and PostgreSQL port.
