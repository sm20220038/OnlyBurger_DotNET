using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Domain.Entities;

namespace OnlyBurger.Api.Data;

/// <summary>
/// Entity Framework Core database context for the OnlyBurger system.
/// Maps the domain entities to SQL Server tables and configures their relationships.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
            // Persist the enum as readable text instead of an int.
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.DeliveryLocation).IsRequired().HasMaxLength(300);
            entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);

            // Don't cascade-delete a product just because it appears on an order line.
            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);

            // One cart per user (1:1). Deleting the user removes their cart.
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Cart)
                  .HasForeignKey<Cart>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.UserId).IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Quantity).IsRequired();

            entity.HasOne(c => c.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(c => c.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Product)
                  .WithMany()
                  .HasForeignKey(c => c.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // A product appears at most once per cart (quantity tracks the count).
            entity.HasIndex(c => new { c.CartId, c.ProductId }).IsUnique();
        });
    }
}
