using Microsoft.EntityFrameworkCore;
using OnlyBurger.Infrastructure.Auth;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;

namespace OnlyBurger.Infrastructure.Data;

/// <summary>
/// Applies pending migrations and seeds baseline data (an admin account and the starter
/// menu) on startup so the API is usable immediately after a fresh checkout.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var config = sp.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        await SeedAdminAsync(db, hasher, config);
        await SeedProductsAsync(db);
        await db.SaveChangesAsync();

        // Give every user that has no active cart a fresh one, so all rows match the current
        // schema. (Active carts are otherwise created lazily on the first "add to cart".)
        await BackfillActiveCartsAsync(db);
    }

    private static async Task BackfillActiveCartsAsync(AppDbContext db)
    {
        var userIdsWithoutActiveCart = await db.Users
            .Where(u => !u.Carts.Any(c => c.Status == CartStatus.Active))
            .Select(u => u.Id)
            .ToListAsync();

        if (userIdsWithoutActiveCart.Count == 0)
        {
            return;
        }

        foreach (var userId in userIdsWithoutActiveCart)
        {
            db.Carts.Add(new Cart { UserId = userId, Status = CartStatus.Active });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        var username = config["SeedAdmin:Username"] ?? "admin";
        var email = config["SeedAdmin:Email"] ?? "admin@onlyburger.com";
        var password = config["SeedAdmin:Password"] ?? "Admin123!";
        var phone = config["SeedAdmin:Phone"] ?? "+381 11 000 0000";

        var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
        if (admin is not null)
        {
            // Admin already exists (e.g. seeded before the phone field existed). Backfill a
            // placeholder phone if it's missing so the delivery contact is never blank.
            if (string.IsNullOrWhiteSpace(admin.PhoneNumber))
            {
                admin.PhoneNumber = phone;
            }

            return;
        }

        db.Users.Add(new User
        {
            Username = username,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = hasher.Hash(password),
            Role = UserRole.Admin,
            // Start the admin off with an empty active cart.
            Carts = new List<Cart> { new() { Status = CartStatus.Active } }
        });
    }

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        // Prices are in Serbian dinar (RSD). The order here matches the product ids that
        // the running database was seeded/extended with, so the menu images in
        // onlyburger-web/public/products/<id>.* line up on a fresh install too.
        db.Products.AddRange(
            new Product { Name = "Classic Burger", Description = "Juneća pljeskavica, zelena salata, paradajz, luk i naš domaći sos.", Price = 630m },
            new Product { Name = "Cheeseburger", Description = "Klasičan burger sa otopljenim čedar sirom.", Price = 650m },
            new Product { Name = "Double Burger", Description = "Dve juneće pljeskavice sa duplim sirom za veći apetit.", Price = 820m },
            new Product { Name = "Chicken Burger", Description = "Hrskav pileći file sa zelenom salatom i majonezom.", Price = 690m },
            new Product { Name = "Fries", Description = "Zlatni, hrskavi pomfrit sa prstohvatom soli.", Price = 270m },
            new Product { Name = "Coca-Cola", Description = "Rashlađena Coca-Cola 0,33L.", Price = 230m },
            new Product { Name = "Veggie Burger", Description = "Pljeskavica na biljnoj bazi sa svežom salatom.", Price = 690m },
            new Product { Name = "Original burger", Description = "100% juneće meso, domaći burger sos, čedar, luk, krastavčići, salata.", Price = 630m },
            new Product { Name = "Caramel burger", Description = "100% juneće meso, karamelizovani luk, burger sos, čedar, luk, krastavčići, salata.", Price = 670m },
            new Product { Name = "Bacon burger", Description = "100% juneće meso, hrskava slanina, burger sos, bbq sos, čedar, luk, paradajz, salata.", Price = 740m },
            new Product { Name = "Jalapeno burger", Description = "100% juneće meso, jalapeno paprika, burger sos, bbq sos, ementaler, čedar, luk, salata.", Price = 740m },
            new Product { Name = "Only burger", Description = "100% juneće meso, hrskava slanina, jaje, burger sos, dimljena mocarela, čedar, luk, paradajz, salata.", Price = 790m },
            new Product { Name = "Onion rings", Description = "7 komada u porciji.", Price = 270m },
            new Product { Name = "Mozzarella sticks", Description = "4 komada u porciji.", Price = 400m }
        );
    }
}
