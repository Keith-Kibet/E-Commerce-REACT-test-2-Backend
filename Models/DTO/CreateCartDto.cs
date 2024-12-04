using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcommApp.Models.DTO
{
    public class CreateCartDto
    {

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        // Quantity will be set to 1 by default in the logic where this DTO is used
        public int Quantity { get; set; } = 1;
    }
}
