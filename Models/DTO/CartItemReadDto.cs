namespace EcommApp.Models.DTO
{
    public class CartItemReadDto
    {

        public int CartId { get; set; }
        public int Quantity { get; set; }

        public string UserId { get; set; } // User ID from session


        public decimal UnitPrice { get; set; }
        // Add other cart item fields if necessary

        // Product details
        public int ProductId { get; set; }

        public decimal Price { get; set; }


        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ImageUrl { get; set; }
        // Add other product fields if necessary

    }
}
