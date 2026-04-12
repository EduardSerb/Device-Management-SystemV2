using DeviceManagement.Api.Data;
using DeviceManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Api.Seed;

public static class DatabaseSeeder
{
    public const string IntegrationTestEnvironment = "IntegrationTest";

    public static async Task MigrateAndSeedAsync(IServiceProvider services, ILogger logger, IHostEnvironment environment, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<ApplicationDbContext>();

        if (environment.IsEnvironment(IntegrationTestEnvironment))
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        await db.Database.MigrateAsync(cancellationToken);

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedUsersAsync(userManager, logger, cancellationToken);
        await SeedDevicesAsync(db, userManager, logger, cancellationToken);
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> users, ILogger logger, CancellationToken cancellationToken)
    {
        if (await users.Users.AnyAsync(cancellationToken))
            return;

        var demoUsers = new[]
        {
            new ApplicationUser
            {
                UserName = "alice@company.test",
                Email = "alice@company.test",
                EmailConfirmed = true,
                FullName = "Alice Johnson",
                RoleName = "Engineer",
                Location = "Berlin"
            },
            new ApplicationUser
            {
                UserName = "bob@company.test",
                Email = "bob@company.test",
                EmailConfirmed = true,
                FullName = "Bob Smith",
                RoleName = "Support",
                Location = "London"
            }
        };

        foreach (var u in demoUsers)
        {
            var result = await users.CreateAsync(u, "Passw0rd!");
            if (!result.Succeeded)
                logger.LogWarning("Failed to seed user {Email}: {Errors}", u.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedDevicesAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Devices.AnyAsync(cancellationToken))
            return;

        var alice = await users.FindByEmailAsync("alice@company.test");
        var bob = await users.FindByEmailAsync("bob@company.test");

        var seed = new List<Device>
        {
            new()
            {
                Name = "iPhone 17 Pro",
                Manufacturer = "Apple",
                Type = DeviceType.Phone,
                OS = "iOS",
                OSVersion = "26.0",
                Processor = "A19 Pro",
                RamAmount = "12GB",
                Description = "A high-performance Apple smartphone running iOS, suitable for daily business use.",
                AssignedUserId = alice?.Id
            },
            new()
            {
                Name = "Pixel 10",
                Manufacturer = "Google",
                Type = DeviceType.Phone,
                OS = "Android",
                OSVersion = "15",
                Processor = "Tensor G4",
                RamAmount = "12GB",
                Description = "Google flagship phone for testing and demos.",
                AssignedUserId = null
            },
            new()
            {
                Name = "Galaxy Tab S10",
                Manufacturer = "Samsung",
                Type = DeviceType.Tablet,
                OS = "Android",
                OSVersion = "15",
                Processor = "Snapdragon 8 Gen 3",
                RamAmount = "12GB",
                Description = "Large screen tablet for field teams.",
                AssignedUserId = bob?.Id
            }
        };

        db.Devices.AddRange(seed);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} demo devices.", seed.Count);
    }
}
