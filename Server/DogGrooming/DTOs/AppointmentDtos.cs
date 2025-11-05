using DogGroomingAPI.Models;

namespace DogGroomingAPI.DTOs;

public class CreateAppointmentDto
{
    public DateTime AppointmentTime { get; set; }
    public DogSize DogSize { get; set; }
}

public class UpdateAppointmentDto
{
    public DateTime AppointmentTime { get; set; }
    public DogSize DogSize { get; set; }
}

public class AppointmentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public string DogSize { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public bool DiscountApplied { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CustomerName { get; set; }
}
