namespace DogGroomingAPI.Models;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public DogSize DogSize { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public bool DiscountApplied { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
}

public enum DogSize
{
    Small = 1,
    Medium = 2,
    Large = 3
}
