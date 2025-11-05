using DogGroomingAPI.Models;

namespace DogGroomingAPI.Services;

public interface IPriceCalculationService
{
    (int duration, decimal price, decimal finalPrice, bool discountApplied) CalculatePrice(DogSize dogSize, int userId);
}
