using DogGroomingAPI.Data;
using DogGroomingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DogGroomingAPI.Services;

public class PriceCalculationService : IPriceCalculationService
{
    private readonly ApplicationDbContext _context;

    public PriceCalculationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public (int duration, decimal price, decimal finalPrice, bool discountApplied) CalculatePrice(
        DogSize dogSize, int userId)
    {
        // Define pricing based on dog size
        var (duration, basePrice) = dogSize switch
        {
            DogSize.Small => (30, 100m),
            DogSize.Medium => (45, 150m),
            DogSize.Large => (60, 200m),
            _ => throw new ArgumentException("Invalid dog size")
        };

        // Check if user has more than 3 previous appointments
        var appointmentCount = _context.Appointments
            .Count(a => a.UserId == userId && a.AppointmentTime < DateTime.UtcNow);

        bool discountApplied = appointmentCount > 3;
        decimal finalPrice = discountApplied ? basePrice * 0.9m : basePrice;

        return (duration, basePrice, finalPrice, discountApplied);
    }
}
