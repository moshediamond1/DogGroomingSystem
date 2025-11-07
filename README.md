# Dog Grooming Appointment System

Full-stack appointment booking system for dog grooming services.

## Tech Stack

- **Frontend:** React + TypeScript + Vite
- **Backend:** .NET 8.0 Web API + Entity Framework Core
- **Database:** SQL Server 2022
- **Containerization:** Docker + Docker Compose

## Features

- JWT authentication & user registration
- Book, view, update, and delete appointments
- Automatic pricing by dog size (Small: ₪100, Medium: ₪150, Large: ₪200)
- 10% loyalty discount after 3+ appointments
- Real-time conflict detection prevents double-bookings
- Transaction-based locking for concurrent users

## Quick Start with Docker (Recommended)

### Prerequisites

- Docker Desktop installed and running

### Start the Application

```bash
docker-compose up -d
```

Automatically starts:

1. SQL Server (with health checks)
2. .NET API server
3. React client

**Access:**

- **Frontend:** http://localhost:3000
- **API:** http://localhost:5000
- **Database:** localhost:1433 (sa / YourStrong@Passw0rd)

### Useful Commands

```bash
docker-compose logs -f              # View logs
docker-compose down                 # Stop all
docker-compose down -v              # Stop and delete database
docker-compose up -d --build        # Rebuild
```

## Manual Setup (Without Docker)

**Prerequisites:** Node.js 18+, .NET 8.0 SDK, SQL Server

### Backend

```bash
cd Server/DogGrooming
dotnet restore
dotnet run                          # Runs on http://localhost:5000
```

Update connection string in `appsettings.json` if needed.

### Frontend

```bash
cd client
npm install
npm run dev                         # Runs on http://localhost:3000
```

## Project Structure

```
HomeAssignment-Swish/
├── client/                 # React frontend (TypeScript + Vite)
│   ├── src/
│   │   ├── components/    # UI components
│   │   ├── pages/         # Page components
│   │   ├── services/      # API calls
│   │   └── context/       # Auth context
│   └── Dockerfile
├── Server/DogGrooming/    # .NET 8 API
│   ├── Controllers/       # API endpoints
│   ├── Services/          # Business logic
│   ├── Models/            # Data models
│   ├── DTOs/              # Data transfer objects
│   ├── Data/              # DbContext
│   └── Dockerfile
└── docker-compose.yml
```

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/validate` - Validate JWT token

### Appointments

- `GET /api/appointments` - Get all appointments (with filters)
- `GET /api/appointments/{id}` - Get appointment by ID
- `POST /api/appointments` - Create new appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Delete appointment

All appointment endpoints require JWT authentication via `Authorization: Bearer <token>` header.

## Pricing

| Dog Size | Duration | Base Price | With Discount (3+ bookings) |
| -------- | -------- | ---------- | --------------------------- |
| Small    | 30 min   | ₪100       | ₪90                         |
| Medium   | 45 min   | ₪150       | ₪135                        |
| Large    | 60 min   | ₪200       | ₪180                        |

## Development Notes

- **Database:** Auto-created on startup with EF Core
- **Database Objects:** View (`vw_AppointmentsWithUsers`) and stored procedure (`sp_CheckOverlappingAppointments`) created automatically in C# code
- **Concurrency:** SERIALIZABLE transaction isolation prevents race conditions
- **JWT:** Tokens expire after 24 hours
- **Deletion:** Appointments scheduled for today cannot be deleted
- **Pricing:** Calculated server-side based on user's appointment history

## Troubleshooting

**SQL Server won't start:**

- Increase Docker memory to 4GB+
- Wait 60s for initialization
- Check logs: `docker-compose logs sqlserver`

**Port conflicts:**

- Port 3000: Stop other apps
- Port 5000: Stop .NET/IIS apps
- Port 1433: Stop local SQL Server

**Connection errors:**

- Check container health: `docker ps`
- Verify connection string matches docker-compose.yml

## License

This project is for educational/assignment purposes.
