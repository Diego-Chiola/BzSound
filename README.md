# BzSound API

A RESTful API for managing user audio tracks, built with ASP.NET Core.

## Features

- JWT-based authentication & authorization
- Upload and manage audio tracks per user
- Supports multiple audio formats: `.mp3`, `.wav`, `.ogg`, `.m4a`, `.flac`
- Max file size: **50MB**

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (recommended) or a local SQL Server instance

### Installation

```bash
git clone https://github.com/Diego-Chiola/BzSound.git
cd BzSound/api
dotnet restore
dotnet run
```

### Database Setup with Docker

The easiest way to get a SQL Server instance running without installing it locally is via Docker.

**1. Start the container:**

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Pass123" \
  -p 1433:1433 --name bzsound-sql -d \
  mcr.microsoft.com/mssql/server:2022-latest
```

**2. Apply EF Core migrations:**

```bash
cd api
dotnet ef database update
```

**Useful Docker commands:**

| Action                 | Command                    |
| ---------------------- | -------------------------- |
| Check container status | `docker ps`                |
| Stop the container     | `docker stop bzsound-sql`  |
| Start it again later   | `docker start bzsound-sql` |
| View logs              | `docker logs bzsound-sql`  |

> **Note:** Use `docker start bzsound-sql` on subsequent runs — no need to recreate the container each time.

### Configuration

Update `appsettings.json` with your database connection string and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BzSoundDB;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SigningKey": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  }
}
```

> **Security:** Avoid committing real credentials. Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:
>
> ```bash
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=BzSoundDB;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True;"
> ```

## API Endpoints

Unless stated otherwise, endpoints require `Authorization: Bearer <token>`.

### Authentication

Base URL: `/api/auth`

| Method | Endpoint    | Auth | Description             |
| ------ | ----------- | ---- | ----------------------- |
| `POST` | `/register` | No   | Register a new account  |
| `POST` | `/login`    | No   | Login and get JWT token |

#### Register Request

`POST /api/auth/register`

```json
{
  "username": "john",
  "email": "john@example.com",
  "password": "StrongPass123!"
}
```

#### Login Request

`POST /api/auth/login`

```json
{
  "email": "john@example.com",
  "password": "StrongPass123!"
}
```

#### Auth Response

```json
{
  "email": "john@example.com",
  "token": "<jwt-token>"
}
```

### Users

Base URL: `/api/users`

| Method   | Endpoint | Auth               | Description                 |
| -------- | -------- | ------------------ | --------------------------- |
| `GET`    | `/`      | Admin              | Get paginated users list    |
| `GET`    | `/{id}`  | Owner or Admin     | Get user by id              |
| `GET`    | `/me`    | Authenticated user | Get current user from token |
| `PUT`    | `/{id}`  | Owner or Admin     | Update user email/password  |
| `DELETE` | `/{id}`  | Owner or Admin     | Delete user                 |

#### Update User Request

`PUT /api/users/{id}`

```json
{
  "email": "newmail@example.com",
  "passwordHash": "NewPassword123!"
}
```

### Tracks

Base URL: `/api/users/{userId}/tracks`

| Method   | Endpoint     | Auth  | Description                                     |
| -------- | ------------ | ----- | ----------------------------------------------- |
| `GET`    | `/`          | Owner | Get user tracks (supports filtering/pagination) |
| `GET`    | `/{trackId}` | Owner | Get a single track                              |
| `POST`   | `/`          | Owner | Create track from uploaded audio                |
| `PUT`    | `/{trackId}` | Owner | Update title and/or replace uploaded file       |
| `DELETE` | `/{trackId}` | Owner | Delete track and associated physical file       |

#### Get Tracks Query Params

- `titleContains` (optional)
- `pageNumber` (default: `1`)
- `pageSize` (default: `20`)
- `sortBy` (optional)
- `isDescending` (default: `false`)

#### Create Track

`POST /api/users/{userId}/tracks`

- **Content-Type:** `multipart/form-data`
- **Form fields:**
  - `file` _(required)_: audio file
  - `title` _(optional, max 150 chars)_: defaults to uploaded filename

Server behavior:

- Validates extension: `.mp3`, `.wav`, `.ogg`, `.m4a`, `.flac`
- Validates max size: `50MB`
- Saves file to `/uploads/{userId}/{generatedName}`
- Computes and stores metadata (file size, format, duration)

#### Update Track

`PUT /api/users/{userId}/tracks/{trackId}`

- **Content-Type:** `multipart/form-data`
- **Form fields (at least one required):**
  - `title` _(optional, max 150 chars)_
  - `file` _(optional)_: new audio file to replace existing one

Server behavior:

- Title-only update modifies DB title only
- File update saves new file, refreshes metadata, and deletes old file

#### Delete Track

`DELETE /api/users/{userId}/tracks/{trackId}`

Server behavior:

- Deletes DB record
- Deletes physical file from `/uploads/...`
