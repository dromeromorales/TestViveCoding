using ProductAPI.Domain.Entities;

namespace ProductAPI.Domain.Specifications;

public static class ProductSpecifications
{
    public static ISpecification<Product> ByPriceRange(decimal minPrice, decimal maxPrice)
    {
        return new ProductByPriceRangeSpecification(minPrice, maxPrice);
    }

    public static ISpecification<Product> ByWeightRange(decimal minWeight, decimal maxWeight)
    {
        return new ProductByWeightRangeSpecification(minWeight, maxWeight);
    }

    public static ISpecification<Product> ByNameOrDescription(string searchTerm)
    {
        return new ProductByNameOrDescriptionSpecification(searchTerm);
    }

    public static ISpecification<Product> ExpensiveProducts(decimal threshold = 1000m)
    {
        return new ExpensiveProductsSpecification(threshold);
    }

    public static ISpecification<Product> LightweightProducts(decimal maxWeight = 5m)
    {
        return new LightweightProductsSpecification(maxWeight);
    }

    public static ISpecification<Product> AffordableAndLightweight(decimal maxPrice = 500m, decimal maxWeight = 5m)
    {
        return new AffordableAndLightweightSpecification(maxPrice, maxWeight);
    }

    public static ISpecification<Product> WithPagination(int pageNumber, int pageSize)
    {
        return new ProductWithPaginationSpecification(pageNumber, pageSize);
    }
}

public class ProductByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
        : base(p => p.Price >= minPrice && p.Price <= maxPrice)
    {
        ApplyOrderBy(p => p.Price);
    }
}

public class ProductByWeightRangeSpecification : BaseSpecification<Product>
{
    public ProductByWeightRangeSpecification(decimal minWeight, decimal maxWeight)
        : base(p => p.Weight >= minWeight && p.Weight <= maxWeight)
    {
        ApplyOrderBy(p => p.Weight);
    }
}

public class ProductByNameOrDescriptionSpecification : BaseSpecification<Product>
{
    public ProductByNameOrDescriptionSpecification(string searchTerm)
        : base(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
    {
        ApplyOrderBy(p => p.Name);
    }
}

public class ExpensiveProductsSpecification : BaseSpecification<Product>
{
    public ExpensiveProductsSpecification(decimal threshold)
        : base(p => p.Price >= threshold)
    {
        ApplyOrderByDescending(p => p.Price);
    }
}

public class LightweightProductsSpecification : BaseSpecification<Product>
{
    public LightweightProductsSpecification(decimal maxWeight)
        : base(p => p.Weight <= maxWeight)
    {
        ApplyOrderBy(p => p.Weight);
    }
}

public class AffordableAndLightweightSpecification : BaseSpecification<Product>
{
    public AffordableAndLightweightSpecification(decimal maxPrice, decimal maxWeight)
        : base(p => p.Price <= maxPrice && p.Weight <= maxWeight)
    {
        ApplyOrderBy(p => p.Price);
    }
}

public class ProductWithPaginationSpecification : BaseSpecification<Product>
{
    public ProductWithPaginationSpecification(int pageNumber, int pageSize)
        : base(p => true)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        ApplyOrderBy(p => p.Name);
    }
}