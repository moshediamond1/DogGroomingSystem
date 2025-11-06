using DogGroomingAPI.DTOs;
using DogGroomingAPI.Models;

namespace DogGroomingAPI.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(DateTime? startDate, DateTime? endDate, string? customerName);
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto, int userId);
    Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto, int userId);
    Task<(bool Success, string? ErrorMessage)> DeleteAppointmentAsync(int id, int userId);
}
