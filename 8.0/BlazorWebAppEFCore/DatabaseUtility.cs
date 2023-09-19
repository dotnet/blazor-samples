using BlazorWebAppEFCore.Data;
using Microsoft.EntityFrameworkCore;

public static class DatabaseUtility
{
    // Method to see the database. Should not be used in production: demo purposes only.
    // options: The configured options.
    // count: The number of contacts to seed.
    public static async Task EnsureDbCreatedAndSeedWithCountOfAsync(DbContextOptions<ContactContext> options, int count)
    {
        // Empty to avoid logging while inserting (otherwise will flood console).
        var factory = new LoggerFactory();
        var builder = new DbContextOptionsBuilder<ContactContext>(options)
            .UseLoggerFactory(factory);

        using var context = new ContactContext(builder.Options);
        // Result is true if the database had to be created.
        if (await context.Database.EnsureCreatedAsync())
        {
            var seed = new SeedContacts();
            await seed.SeedDatabaseWithContactCountOfAsync(context, count);
        }
    }
}
