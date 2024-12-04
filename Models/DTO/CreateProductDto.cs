namespace EcommApp.Models.DTO
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int UnitsAvailable { get; set; }

        public IFormFile ImageFile { get; set; }

    }

}
