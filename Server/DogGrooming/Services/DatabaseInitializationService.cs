using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DogGroomingAPI.Data;

namespace DogGroomingAPI.Services;

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseInitializationService(
        ApplicationDbContext context,
        ILogger<DatabaseInitializationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");

            await _context.Database.EnsureCreatedAsync();
            await CreateDatabaseObjectsDirectly();

            _logger.LogInformation("Database initialization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    private async Task CreateDatabaseObjectsDirectly()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var scripts = new[]
        {
            "DROP VIEW IF EXISTS vw_AppointmentsWithUsers",
            @"CREATE VIEW vw_AppointmentsWithUsers AS
              SELECT a.Id, a.UserId, a.AppointmentTime, a.DogSize, a.DurationMinutes,
                     a.Price, a.FinalPrice, a.DiscountApplied, a.CreatedAt, u.FirstName AS CustomerName
              FROM Appointments a INNER JOIN Users u ON a.UserId = u.Id",
            "DROP PROCEDURE IF EXISTS sp_CheckOverlappingAppointments",
            @"CREATE PROCEDURE sp_CheckOverlappingAppointments
                  @AppointmentTime DATETIME2, @DurationMinutes INT, @ExcludeAppointmentId INT = NULL
              AS BEGIN SET NOCOUNT ON;
                  DECLARE @EndTime DATETIME2 = DATEADD(MINUTE, @DurationMinutes, @AppointmentTime);
                  SELECT CAST(CASE WHEN EXISTS (
                      SELECT 1 FROM Appointments
                      WHERE (@ExcludeAppointmentId IS NULL OR Id != @ExcludeAppointmentId)
                        AND ((@AppointmentTime >= AppointmentTime AND @AppointmentTime < DATEADD(MINUTE, DurationMinutes, AppointmentTime))
                          OR (@EndTime > AppointmentTime AND @EndTime <= DATEADD(MINUTE, DurationMinutes, AppointmentTime))
                          OR (@AppointmentTime <= AppointmentTime AND @EndTime >= DATEADD(MINUTE, DurationMinutes, AppointmentTime)))
                  ) THEN 1 ELSE 0 END AS INT) AS HasOverlap;
              END"
        };

        foreach (var script in scripts)
        {
            using var cmd = new SqlCommand(script, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogInformation("View and stored procedure created");
    }
}
