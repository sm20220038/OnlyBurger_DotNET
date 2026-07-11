using OnlyBurger.Infrastructure.Data.Repositories;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data;

/// <summary>
/// EF Core Unit of Work. Owns one <see cref="AppDbContext"/> that every repository shares, so
/// changes made through different repositories are committed together by <see cref="SaveChangesAsync"/>.
/// Repositories are created lazily on first access.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private ICartRepository? _carts;
    private ICartItemRepository? _cartItems;
    private IUserRepository? _users;
    private IStudentRepository? _students;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public ICartRepository Carts => _carts ??= new CartRepository(_context);
    public ICartItemRepository CartItems => _cartItems ??= new CartItemRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IStudentRepository Students => _students ??= new StudentRepository(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
