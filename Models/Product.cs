using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcommApp.Models
{
    public class Product
    {

            [Key]
            public int Id { get; set; }

             [Required]
            [MaxLength(255)]
            public string ProductName { get; set; }

            [Required]
            [MaxLength(1000)]
            public string ProductDescription { get; set; }

            [Required]
            [Column(TypeName = "decimal(18, 2)")]
            public decimal Price { get; set; }

            [Required]
            public int UnitsAvailable { get; set; }

            public string ImageUrl { get; set; } // Path to the product image

        [NotMapped]
        public IFormFile ImageFile { get; set; }
        



    }
}
