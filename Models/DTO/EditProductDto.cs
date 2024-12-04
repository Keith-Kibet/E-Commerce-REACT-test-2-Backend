namespace EcommApp.Models.DTO
{
    public class EditProductDto
    {
        public string? ProductName { get; set; } // Not required, update if provided
        public string? ProductDescription { get; set; } // Not required, update if provided
        public decimal? Price { get; set; } // Nullable, update if provided
        public int? UnitsAvailable { get; set; } // Nullable, update if provided
        public IFormFile? ImageFile { get; set; }

    }
}
