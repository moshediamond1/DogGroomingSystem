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

        // Calculate price with discount
        var (duration, price, finalPrice, discountApplied) = 
            _priceService.CalculatePrice(dto.DogSize, userId);

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

        // Reload with user data
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var userId = GetUserId();
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        // Check if user owns this appointment
        if (appointment.UserId != userId)
        {
            return Forbid();
        }

        // Recalculate price with potential discount
        var (duration, price, finalPrice, discountApplied) = 
            _priceService.CalculatePrice(dto.DogSize, userId);

        appointment.AppointmentTime = dto.AppointmentTime;
        appointment.DogSize = dto.DogSize;
        appointment.DurationMinutes = duration;
        appointment.Price = price;
        appointment.FinalPrice = finalPrice;
        appointment.DiscountApplied = discountApplied;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var userId = GetUserId();
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        // Check if user owns this appointment
        if (appointment.UserId != userId)
        {
            return Forbid();
        }

        // Check if appointment is for today
        if (appointment.AppointmentTime.Date == DateTime.Today)
        {
            return BadRequest(new { message = "Cannot delete appointments scheduled for today" });
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
