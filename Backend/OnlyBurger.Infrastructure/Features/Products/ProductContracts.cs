using System.ComponentModel.DataAnnotations;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Product as returned to clients.</summary>
public record ProductDto(int Id, string Name, string Description, decimal Price);

/// <summary>Request body for creating a menu product (admin only).</summary>
public record CreateProductRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; init; }
}

/// <summary>Request body for updating an existing menu product (admin only).</summary>
public record UpdateProductRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; init; }
}
