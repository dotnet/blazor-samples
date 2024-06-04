using System.ComponentModel.DataAnnotations;
using Humanizer;

namespace BlazorWebAppMovies.Models
{
    public class Movie
    {
        public int Id { get; set; }

        /* The data annotations for the next property
           are added in Part 5 of the tutorial series.
        */
        [Required, StringLength(60, MinimumLength = 3)]
        public string? Title { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        /* The data annotations for the next property
           are added in Part 5 of the tutorial series.
        */
        [RegularExpression(@"^[A-Z]+[a-zA-Z()\s-]*$"), Required, StringLength(30)]
        public string? Genre { get; set; }

        /* The data annotations for the next property
           are added in Part 5 of the tutorial series.
        */
        [Range(1, 100)]
        public decimal Price { get; set; }

        /* The following property is added in Part 8 of
           the tutorial series.
        */
        [RegularExpression(@"^(G|PG|PG-13|R|NC-17)$"), Required]
        public string? Rating { get; set; }
    }
}
