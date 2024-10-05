namespace BlazorWebAppEFCore.Data;

// Generates desired number of random contacts.
public class SeedContacts
{
    // Use these to make names.
    private readonly string[] gems = [
        "Diamond",
        "Crystal",
        "Morion",
        "Azore",
        "Sapphire",
        "Cobalt",
        "Aquamarine",
        "Montana",
        "Turquoise",
        "Lime",
        "Erinite",
        "Emerald",
        "Turmaline",
        "Jonquil",
        "Olivine",
        "Topaz",
        "Citrine",
        "Sun",
        "Quartz",
        "Opal",
        "Alabaster",
        "Rose",
        "Burgundy",
        "Siam",
        "Ruby",
        "Amethyst",
        "Violet",
        "Lilac"];

    // Combined with things for last names.
    private readonly string[] colors =
    [
        "Blue",
        "Aqua",
        "Red",
        "Green",
        "Orange",
        "Yellow",
        "Black",
        "Violet",
        "Brown",
        "Crimson",
        "Gray",
        "Cyan",
        "Magenta",
        "White",
        "Gold",
        "Pink",
        "Lavender"
    ];

    // Also helpful for names.
    private readonly string[] things =
    [
        "beard",
        "finger",
        "hand",
        "toe",
        "stalk",
        "hair",
        "vine",
        "street",
        "son",
        "brook",
        "river",
        "lake",
        "stone",
        "ship"
    ];

    // Street names.
    private readonly string[] streets =
    [
        "Broad",
        "Wide",
        "Main",
        "Pine",
        "Ash",
        "Poplar",
        "First",
        "Third",
    ];

    // Types of streets.
    private readonly string[] streetTypes =
    [
        "Street",
        "Lane",
        "Place",
        "Terrace",
        "Drive",
        "Way"
    ];

    // More uniqueness.
    private readonly string[] directions =
    [
        "N",
        "NE",
        "E",
        "SE",
        "S",
        "SW",
        "W",
        "NW"
    ];

    // A sampling of cities.
    private readonly string[] cities =
    [
        "Austin",
        "Denver",
        "Fayetteville",
        "Des Moines",
        "San Francisco",
        "Portland",
        "Monroe",
        "Redmond",
        "Bothel",
        "Woodinville",
        "Kent",
        "Kennesaw",
        "Marietta",
        "Atlanta",
        "Lead",
        "Spokane",
        "Bellevue",
        "Seattle"
    ];

    // State list.
    private readonly string[] states =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL",
        "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA",
        "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE",
        "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK",
        "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT",
        "VA", "WA", "WV", "WI", "WY"
    ];

    // Picks a random item from a list.
    // list: A list of string to parse.
    private static string RandomOne(string[] list)
    {
        var idx = Random.Shared.Next(list.Length - 1);

        return list[idx];
    }

    // Make a new contact.
    // Returns a random Contact instance.
    private Contact MakeContact()
    {
        var contact = new Contact
        {
            FirstName = RandomOne(gems),
            LastName = $"{RandomOne(colors)}{RandomOne(things)}",
            Phone = $"({Random.Shared.Next(100, 999)})-555-{Random.Shared.Next(1000, 9999)}",
            Street = $"{Random.Shared.Next(1, 99999)} {Random.Shared.Next(1, 999)}" +
            $" {RandomOne(streets)} {RandomOne(streetTypes)} {RandomOne(directions)}",
            City = RandomOne(cities),
            State = RandomOne(states),
            ZipCode = $"{Random.Shared.Next(10000, 99999)}"
        };

        return contact;
    }

    public async Task SeedDatabaseWithContactCountOfAsync(ContactContext context, int totalCount)
    {
        var count = 0;
        var currentCycle = 0;
        while (count < totalCount)
        {
            var list = new List<Contact>();
            while (currentCycle++ < 100 && count++ < totalCount)
            {
                list.Add(MakeContact());
            }
            if (list.Count > 0)
            {
                context.Contacts?.AddRange(list);
                _ = await context.SaveChangesAsync();
            }
            currentCycle = 0;
        }
    }
}
