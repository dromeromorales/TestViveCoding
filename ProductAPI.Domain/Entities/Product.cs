using ProductAPI.Domain.Exceptions;

namespace ProductAPI.Domain.Entities;

public class Product
{
    private const decimal MaxPrice = 10000m;
    private const decimal MaxWeight = 80m;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal Weight { get; private set; }

    private Product(Guid id, string name, string description, decimal price, decimal weight)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Weight = weight;
    }

    public static Product Create(Guid id, string name, string description, decimal price, decimal weight)
    {
        ValidateBusinessRules(price, weight);
        ValidateRequiredFields(name, description);

        return new Product(id, name, description, price, weight);
    }

    private static void ValidateBusinessRules(decimal price, decimal weight)
    {
        if (price > MaxPrice)
            throw new InvalidPriceException($"Price cannot exceed ${MaxPrice}");

        if (weight > MaxWeight)
            throw new InvalidWeightException($"Weight cannot exceed {MaxWeight}kg");

        if (price < 0)
            throw new InvalidPriceException("Price cannot be negative");

        if (weight < 0)
            throw new InvalidWeightException("Weight cannot be negative");
    }

    private static void ValidateRequiredFields(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required");
    }
}