using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppEFCore.Data;

// Context for the contacts database.
public class ContactContext : DbContext
{
    // Magic string.
    public static readonly string RowVersion = nameof(RowVersion);

    // Magic strings.
    public static readonly string ContactsDb = nameof(ContactsDb).ToLower();

    // Inject options.
    // options: The DbContextOptions{ContactContext} for the context.
    public ContactContext(DbContextOptions<ContactContext> options)
        : base(options)
    {
        Debug.WriteLine($"{ContextId} context created.");
    }

    // List of Contact.
    public DbSet<Contact>? Contacts { get; set; }

    // Define the model.
    // modelBuilder: The ModelBuilder.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This property isn't on the C# class,
        // so we set it up as a "shadow" property and use it for concurrency.
        modelBuilder.Entity<Contact>()
            .Property<byte[]>(RowVersion)
            .IsRowVersion();

        base.OnModelCreating(modelBuilder);
    }

    // Dispose pattern.
    public override void Dispose()
    {
        Debug.WriteLine($"{ContextId} context disposed.");
        base.Dispose();
    }

    // Dispose pattern.
    public override ValueTask DisposeAsync()
    {
        Debug.WriteLine($"{ContextId} context disposed async.");
        return base.DisposeAsync();
    }
}
