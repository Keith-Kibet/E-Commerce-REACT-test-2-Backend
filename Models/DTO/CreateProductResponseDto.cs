namespace EcommApp.Models.DTO
{
    public class CreateProductResponseDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int UnitsAvailable { get; set; }
        public string ImageUrl { get; set; }
    }
}
