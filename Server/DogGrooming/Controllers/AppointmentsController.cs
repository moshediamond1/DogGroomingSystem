using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DogGroomingAPI.DTOs;
using DogGroomingAPI.Services;

namespace DogGroomingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
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
        var appointments = await _appointmentService.GetAppointmentsAsync(startDate, endDate, customerName);
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found" });

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto dto)
    {
        var userId = GetUserId();

        try
        {
            var appointment = await _appointmentService.CreateAppointmentAsync(dto, userId);
            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var userId = GetUserId();

        try
        {
            var success = await _appointmentService.UpdateAppointmentAsync(id, dto, userId);

            if (!success)
                return NotFound(new { message = "Appointment not found or access denied" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var userId = GetUserId();
        var (success, errorMessage) = await _appointmentService.DeleteAppointmentAsync(id, userId);

        if (!success)
        {
            return errorMessage switch
            {
                "Appointment not found" => NotFound(new { message = errorMessage }),
                "Unauthorized" => Forbid(),
                "Cannot delete appointments scheduled for today" => BadRequest(new { message = errorMessage }),
                _ => BadRequest(new { message = errorMessage })
            };
        }

        return NoContent();
    }
}
