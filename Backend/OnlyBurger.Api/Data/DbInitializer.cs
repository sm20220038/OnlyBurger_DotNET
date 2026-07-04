using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Domain.Entities;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Data;

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

        // Older user rows were created before the Cart entity existed. Give every user that
        // is missing a cart one now, so all rows match the current schema (1 cart per user).
        await BackfillCartsAsync(db);
    }

    private static async Task BackfillCartsAsync(AppDbContext db)
    {
        var userIdsWithoutCart = await db.Users
            .Where(u => u.Cart == null)
            .Select(u => u.Id)
            .ToListAsync();

        if (userIdsWithoutCart.Count == 0)
        {
            return;
        }

        foreach (var userId in userIdsWithoutCart)
        {
            db.Carts.Add(new Cart { UserId = userId });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            return;
        }

        var username = config["SeedAdmin:Username"] ?? "admin";
        var email = config["SeedAdmin:Email"] ?? "admin@onlyburger.com";
        var password = config["SeedAdmin:Password"] ?? "Admin123!";

        db.Users.Add(new User
        {
            Username = username,
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = UserRole.Admin,
            // Every user owns exactly one cart; create it up front.
            Cart = new Cart()
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
