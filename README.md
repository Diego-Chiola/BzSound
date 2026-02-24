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
- A supported database (configured via `appsettings.json`)

### Installation

```bash
git clone https://github.com/your-username/BzSound.git
cd BzSound/api
dotnet restore
dotnet run
```

### Configuration

Update `appsettings.json` with your database connection string and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  }
}
```

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
