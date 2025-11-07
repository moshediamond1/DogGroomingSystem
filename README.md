# Dog Grooming Appointment System

A full-stack appointment booking system for dog grooming services with features including user authentication, appointment scheduling with conflict prevention, dynamic pricing with loyalty discounts, and real-time availability checking.

## Tech Stack

**Frontend:** React + TypeScript + Vite
**Backend:** .NET 8.0 Web API + Entity Framework Core
**Database:** SQL Server 2022
**Containerization:** Docker + Docker Compose

## Features

- User registration and JWT authentication
- Create, view, update, and delete appointments
- Automatic pricing based on dog size (Small/Medium/Large)
- 10% loyalty discount after 3+ completed appointments
- Concurrent booking prevention with transaction locking
- Real-time appointment conflict detection

## Quick Start with Docker (Recommended)

### Prerequisites
- Docker Desktop installed and running

### Run Everything

**Windows:**
```bash
start.bat
```

**Mac/Linux:**
```bash
bash start.sh
```

Or manually:
```bash
docker-compose up -d
```

The app will automatically:
1. Start SQL Server with health checks
2. Build and run the .NET API server
3. Build and run the React client
4. Open http://localhost:3000 in your browser

### Access Points
- **Client (Frontend):** http://localhost:3000
- **API (Backend):** http://localhost:5000
- **SQL Server:** localhost:1433 (sa / YourStrong@Passw0rd)

### Docker Commands
```bash
# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove database (fresh start)
docker-compose down -v

# Rebuild containers
docker-compose up -d --build
```

## Manual Setup (Without Docker)

### Prerequisites
- Node.js 18+ and npm
- .NET 8.0 SDK
- SQL Server (LocalDB or SQL Server Express)

### Backend Setup
1. Start SQL Server (LocalDB or SQL Server Express)
2. Update connection string in `Server/DogGrooming/appsettings.json` if needed

```bash
cd Server/DogGrooming

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json if needed
# Run the API
dotnet run
```

API will run on: http://localhost:5000

### Frontend Setup
```bash
cd client

# Install dependencies
npm install

# Start development server
npm run dev
```

Client will run on: http://localhost:3000

## Project Structure

```
HomeAssignment-Swish/
├── client/                 # React frontend
│   ├── src/
│   │   ├── components/    # React components
│   │   ├── context/       # Auth context
│   │   ├── pages/         # Page components
│   │   ├── services/      # API service layer
│   │   └── types/         # TypeScript types
│   └── Dockerfile
├── Server/
│   └── DogGrooming/       # .NET API
│       ├── Controllers/   # API endpoints
│       ├── Services/      # Business logic
│       ├── Models/        # Data models
│       ├── Data/          # DbContext
│       └── Dockerfile
├── docker-compose.yml
├── start.bat              # Windows startup script
└── start.sh               # Mac/Linux startup script
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
|----------|----------|------------|----------------------------|
| Small    | 30 min   | ₪100       | ₪90                        |
| Medium   | 45 min   | ₪150       | ₪135                       |
| Large    | 60 min   | ₪200       | ₪180                       |

## Development Notes

- Database schema, views, and stored procedures are auto-created on app startup via `Program.cs`
- SQL Server uses pessimistic locking (UPDLOCK, HOLDLOCK) to prevent double-bookings
- Appointments use SERIALIZABLE transaction isolation for race condition prevention
- JWT tokens expire after 24 hours
- Appointments scheduled for today cannot be deleted
- The system uses SQL view (`vw_AppointmentDetails`) for optimized appointment queries
- The system uses stored procedure (`sp_CheckAppointmentOverlap`) for conflict detection

## Troubleshooting

**SQL Server container fails to start:**
- Increase Docker memory to at least 4GB
- Wait 60 seconds for SQL Server to fully initialize
- Check logs: `docker-compose logs sqlserver`

**Port already in use:**
- Client (3000): Stop other apps using port 3000
- Server (5000): Stop other .NET apps or IIS
- SQL (1433): Stop local SQL Server instances

**Database connection errors:**
- Ensure SQL Server container is healthy: `docker ps`
- Verify connection string in docker-compose.yml matches appsettings

## License

This project is for educational/assignment purposes.
