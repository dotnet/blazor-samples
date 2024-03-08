namespace BlazorWebAppEFCore.Data;

// Generates desired number of random contacts.
public class SeedContacts
{
    // Use these to make names.
    private readonly string[] _gems = new[] {
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
        "Lilac"};

    // Combined with things for last names.
    private readonly string[] _colors = new[]
    {
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
    };

    // Also helpful for names.
    private readonly string[] _things = new[]
    {
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
    };

    // Street names.
    private readonly string[] _streets = new[]
    {
        "Broad",
        "Wide",
        "Main",
        "Pine",
        "Ash",
        "Poplar",
        "First",
        "Third",
    };

    // Types of streets.
    private readonly string[] _streetTypes = new[]
    {
        "Street",
        "Lane",
        "Place",
        "Terrace",
        "Drive",
        "Way"
    };

    // More uniqueness.
    private readonly string[] _directions = new[]
    {
        "N",
        "NE",
        "E",
        "SE",
        "S",
        "SW",
        "W",
        "NW"
    };

    // A sampling of cities.
    private readonly string[] _cities = new[]
    {
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
    };

    // State list.
    private readonly string[] _states = new[]
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL",
        "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA",
        "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE",
        "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK",
        "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT",
        "VA", "WA", "WV", "WI", "WY"
    };

    // Picks a random item from a list.
    // list: A list of string to parse.
    private string RandomOne(string[] list)
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
            FirstName = RandomOne(_gems),
            LastName = $"{RandomOne(_colors)}{RandomOne(_things)}",
            Phone = $"({Random.Shared.Next(100, 999)})-555-{Random.Shared.Next(1000, 9999)}",
            Street = $"{Random.Shared.Next(1, 99999)} {Random.Shared.Next(1, 999)}" +
            $" {RandomOne(_streets)} {RandomOne(_streetTypes)} {RandomOne(_directions)}",
            City = RandomOne(_cities),
            State = RandomOne(_states),
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
                await context.SaveChangesAsync();
            }
            currentCycle = 0;
        }
    }
}
