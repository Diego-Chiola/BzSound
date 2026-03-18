# BzSound API

A RESTful API for managing user audio tracks, built with ASP.NET Core.

## Features

- JWT-based authentication & authorization
- Upload and manage audio tracks per user
- Supports multiple audio formats: `.mp3`, `.wav`, `.ogg`, `.m4a`, `.flac`
- Max file size: **50MB**

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
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

## API Endpoints (not definitive)

All endpoints require a valid JWT Bearer token.

Base URL: `/api/users/{userId}/tracks`

| Method   | Endpoint     | Description                             |
| -------- | ------------ | --------------------------------------- |
| `GET`    | `/`          | Get all tracks for a user               |
| `GET`    | `/{trackId}` | Get a specific track by ID              |
| `POST`   | `/`          | Create a track (with file path)         |
| `POST`   | `/upload`    | Upload an audio file and create a track |
| `PUT`    | `/{trackId}` | Update track metadata                   |
| `DELETE` | `/{trackId}` | Delete a track                          |

### Upload a Track

`POST /api/users/{userId}/tracks/upload`

- **Content-Type:** `multipart/form-data`
- **Form fields:**
  - `file` _(required)_: Audio file
  - `title` _(optional)_: Track title (defaults to filename)
