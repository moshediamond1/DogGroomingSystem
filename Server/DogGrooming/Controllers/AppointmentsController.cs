using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DogGroomingAPI.Data;
using DogGroomingAPI.DTOs;
using DogGroomingAPI.Models;
using DogGroomingAPI.Services;

namespace DogGroomingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPriceCalculationService _priceService;

    public AppointmentsController(
        ApplicationDbContext context, 
        IPriceCalculationService priceService)
    {
        _context = context;
        _priceService = priceService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    private async Task<bool> HasOverlappingAppointmentsAsync(
        DateTime appointmentTime,
        int durationMinutes,
        int? excludeAppointmentId = null)
    {
        var requestedStart = appointmentTime;
        var requestedEnd = appointmentTime.AddMinutes(durationMinutes);

        // Use UPDLOCK to lock the rows being checked, preventing concurrent modifications
        var overlappingCount = await _context.Appointments
            .FromSqlRaw(@"
                SELECT * FROM Appointments WITH (UPDLOCK, HOLDLOCK)
                WHERE (@excludeId IS NULL OR Id != @excludeId)
                AND (
                    (@start >= AppointmentTime AND @start < DATEADD(MINUTE, DurationMinutes, AppointmentTime))
                    OR (@end > AppointmentTime AND @end <= DATEADD(MINUTE, DurationMinutes, AppointmentTime))
                    OR (@start <= AppointmentTime AND @end >= DATEADD(MINUTE, DurationMinutes, AppointmentTime))
                )",
                new Microsoft.Data.SqlClient.SqlParameter("@start", requestedStart),
                new Microsoft.Data.SqlClient.SqlParameter("@end", requestedEnd),
                new Microsoft.Data.SqlClient.SqlParameter("@excludeId", (object?)excludeAppointmentId ?? DBNull.Value))
            .CountAsync();

        return overlappingCount > 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? customerName)
    {
        var query = _context.Appointments
            .Include(a => a.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.AppointmentTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.AppointmentTime <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query = query.Where(a => a.User.FirstName.Contains(customerName));
        }

        var appointments = await query
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                UserId = a.UserId,
                CustomerName = a.User.FirstName,
                AppointmentTime = a.AppointmentTime,
                DogSize = a.DogSize.ToString(),
                DurationMinutes = a.DurationMinutes,
                Price = a.Price,
                FinalPrice = a.FinalPrice,
                DiscountApplied = a.DiscountApplied,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        return Ok(new AppointmentDto
        {
            Id = appointment.Id,
            UserId = appointment.UserId,
            CustomerName = appointment.User.FirstName,
            AppointmentTime = appointment.AppointmentTime,
            DogSize = appointment.DogSize.ToString(),
            DurationMinutes = appointment.DurationMinutes,
            Price = appointment.Price,
            FinalPrice = appointment.FinalPrice,
            DiscountApplied = appointment.DiscountApplied,
            CreatedAt = appointment.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto dto)
    {
        var userId = GetUserId();

        // Use transaction with serializable isolation level to prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var (duration, price, finalPrice, discountApplied) =
                _priceService.CalculatePrice(dto.DogSize, userId);

            // Check for overlapping appointments with row-level locking
            if (await HasOverlappingAppointmentsAsync(dto.AppointmentTime, duration))
            {
                return Conflict(new { message = "This time slot is already booked. Please choose another time." });
            }

            var appointment = new Appointment
            {
                UserId = userId,
                AppointmentTime = dto.AppointmentTime,
                DogSize = dto.DogSize,
                DurationMinutes = duration,
                Price = price,
                FinalPrice = finalPrice,
                DiscountApplied = discountApplied,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _context.Entry(appointment).Reference(a => a.User).LoadAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id },
                new AppointmentDto
                {
                    Id = appointment.Id,
                    UserId = appointment.UserId,
                    CustomerName = appointment.User.FirstName,
                    AppointmentTime = appointment.AppointmentTime,
                    DogSize = appointment.DogSize.ToString(),
                    DurationMinutes = appointment.DurationMinutes,
                    Price = appointment.Price,
                    FinalPrice = appointment.FinalPrice,
                    DiscountApplied = appointment.DiscountApplied,
                    CreatedAt = appointment.CreatedAt
                });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var userId = GetUserId();

        // Use transaction with serializable isolation level to prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            if (appointment.UserId != userId)
            {
                return Forbid();
            }

            var (duration, price, finalPrice, discountApplied) =
                _priceService.CalculatePrice(dto.DogSize, userId);

            // Check for overlapping appointments (excluding the current appointment)
            if (await HasOverlappingAppointmentsAsync(dto.AppointmentTime, duration, id))
            {
                return Conflict(new { message = "This time slot is already booked. Please choose another time." });
            }

            appointment.AppointmentTime = dto.AppointmentTime;
            appointment.DogSize = dto.DogSize;
            appointment.DurationMinutes = duration;
            appointment.Price = price;
            appointment.FinalPrice = finalPrice;
            appointment.DiscountApplied = discountApplied;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var userId = GetUserId();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            if (appointment.UserId != userId)
            {
                return Forbid();
            }

            if (appointment.AppointmentTime.Date == DateTime.Today)
            {
                return BadRequest(new { message = "Cannot delete appointments scheduled for today" });
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
