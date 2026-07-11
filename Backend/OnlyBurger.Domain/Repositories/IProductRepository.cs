using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>Repository for menu products. The generic operations are enough for products.</summary>
public interface IProductRepository : IRepository<Product>
{
}
