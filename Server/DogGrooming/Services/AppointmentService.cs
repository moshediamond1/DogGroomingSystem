using Microsoft.EntityFrameworkCore;
using DogGroomingAPI.Data;
using DogGroomingAPI.DTOs;
using DogGroomingAPI.Models;

namespace DogGroomingAPI.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IPriceCalculationService _priceService;

    public AppointmentService(ApplicationDbContext context, IPriceCalculationService priceService)
    {
        _context = context;
        _priceService = priceService;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? customerName)
    {
        var query = _context.AppointmentsView
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.AppointmentTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.AppointmentTime <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(customerName))
            query = query.Where(a => a.CustomerName.Contains(customerName));

        var appointments = await query
            .OrderBy(a => a.AppointmentTime)
            .Select(a => MapViewToDto(a))
            .ToListAsync();

        return appointments;
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _context.AppointmentsView
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapViewToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var (duration, price, finalPrice, discountApplied) =
                _priceService.CalculatePrice(dto.DogSize, userId);

            if (await HasOverlappingAppointmentsAsync(dto.AppointmentTime, duration))
            {
                throw new InvalidOperationException("This time slot is already booked. Please choose another time.");
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

            return MapToDto(appointment);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null || appointment.UserId != userId)
                return false;

            var (duration, price, finalPrice, discountApplied) =
                _priceService.CalculatePrice(dto.DogSize, userId);

            if (await HasOverlappingAppointmentsAsync(dto.AppointmentTime, duration, id))
            {
                throw new InvalidOperationException("This time slot is already booked. Please choose another time.");
            }

            appointment.AppointmentTime = dto.AppointmentTime;
            appointment.DogSize = dto.DogSize;
            appointment.DurationMinutes = duration;
            appointment.Price = price;
            appointment.FinalPrice = finalPrice;
            appointment.DiscountApplied = discountApplied;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAppointmentAsync(int id, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return (false, "Appointment not found");

            if (appointment.UserId != userId)
                return (false, "Unauthorized");

            if (appointment.AppointmentTime.Date == DateTime.Today)
                return (false, "Cannot delete appointments scheduled for today");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<bool> HasOverlappingAppointmentsAsync(
        DateTime appointmentTime,
        int durationMinutes,
        int? excludeAppointmentId = null)
    {
        var result = await _context.Database.SqlQueryRaw<int>(
            "EXEC sp_CheckOverlappingAppointments @p0, @p1, @p2",
            appointmentTime,
            durationMinutes,
            excludeAppointmentId ?? (object)DBNull.Value
        ).ToListAsync();

        return result.FirstOrDefault() == 1;
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
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
        };
    }

    private static AppointmentDto MapViewToDto(dynamic viewModel)
    {
        return new AppointmentDto
        {
            Id = viewModel.Id,
            UserId = viewModel.UserId,
            CustomerName = viewModel.CustomerName,
            AppointmentTime = viewModel.AppointmentTime,
            DogSize = viewModel.DogSize.ToString(),
            DurationMinutes = viewModel.DurationMinutes,
            Price = viewModel.Price,
            FinalPrice = viewModel.FinalPrice,
            DiscountApplied = viewModel.DiscountApplied,
            CreatedAt = viewModel.CreatedAt
        };
    }
}