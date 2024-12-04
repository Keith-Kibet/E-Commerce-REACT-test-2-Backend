using EcommApp.Data;
using EcommApp.Exceptions;
using EcommApp.Models;
using EcommApp.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EcommApp.Repository
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly EcommDbContext context;

        public CartItemRepository(EcommDbContext context)
        {
            this.context = context;
        }

        public async Task<Cart> AddItemToCartAsync(CreateCartDto createCartDto)
        {


            // Check if the item already exists in the cart
            var existingCartItem = await context.Carts
                .FirstOrDefaultAsync(c => c.ProductId == createCartDto.ProductId && c.UserId == createCartDto.UserId);

            if (existingCartItem == null)
            {
                // Item does not exist, add new item to cart
                var newCartItem = new Cart
                {
                    UserId = createCartDto.UserId,
                    ProductId = createCartDto.ProductId,
                    Quantity = createCartDto.Quantity, // Assuming Quantity comes from DTO, defaults to 1
                    UnitPrice = createCartDto.UnitPrice
                };

                context.Carts.Add(newCartItem);
                await context.SaveChangesAsync();

                return newCartItem;
            }

            // Return null or handle the case when the item already exists in the cart
            return null;

        }

        public async Task<IEnumerable<Cart>> GetCartItemsByUserIdAsync(string userId)
        {

            var cartItems = await context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product) // Optionally include Product details
                .ToListAsync();

            return cartItems;

        }

        public async Task<CartItemsSummation> GetCartTotalsAsync(string userId)
        {
            // Check if the user has any cart items
            var cartItems = await context.Carts
                                            .Where(cart => cart.UserId == userId)
                                            .ToListAsync();

            // If no items are found for the user, return zero values
            if (!cartItems.Any())
            {
                return new CartItemsSummation
                {
                    TotalItems = 0,
                    TotalCost = 0
                };
            }

            // Calculate the total items and total cost
            var totalItems = cartItems.Sum(item => item.Quantity);
            var totalCost = cartItems.Sum(item => item.Quantity * item.UnitPrice);

            return new CartItemsSummation
            {
                TotalItems = totalItems,
                TotalCost = totalCost
            };
        }

        public  async Task<bool> RemoveAllItemsFromCartAsync(string userId)
        {

            // Retrieve all cart items for the user
            var cartItems = await context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                // If there are items, remove them
                context.Carts.RemoveRange(cartItems);
                await context.SaveChangesAsync();
                return true;
            }

            // Return false if there were no items to remove
            return false;

        }

        public async Task<bool> RemoveItemFromCartAsync(int productId, string userId)
        {
            // Find the cart item that matches the productId and userId
            var cartItem = await context.Carts
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                // If the item exists, remove it from the context and save changes
                context.Carts.Remove(cartItem);
                await context.SaveChangesAsync();
                return true;
            }

            // Return false if no matching item was found
            return false;

        }

        public async Task<CartItemReadDto> UpdateCartItemQuantityAsync(int productId, string userId, int newQuantity)
        {
            var cartItem = await context.Carts
                                        .Include(c => c.Product)
                                        .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                if (newQuantity >= cartItem.Product.UnitsAvailable)
                {
                    throw new StockLimitExceededException("Requested quantity exceeds available stock.");
                }

                cartItem.Quantity = newQuantity + 1;
                await context.SaveChangesAsync();

                // Map Cart to CartItemReadDto
                var cartItemDto = new CartItemReadDto
                {
                    CartId = cartItem.CartId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    ProductId = cartItem.Product.Id,
                    Price = cartItem.Product.Price,
                    ProductName = cartItem.Product.ProductName,
                    ProductDescription = cartItem.Product.ProductDescription,
                    ImageUrl = cartItem.Product.ImageUrl
                };

                return cartItemDto;
            }

            return null;
        }






        public async Task<CartItemReadDto> UpdateCartItemQuantityDeductAsync(int productId, string userId, int newQuantity)
        {


            var cartItem = await context.Carts
                                        .Include(c => c.Product)
                                        .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null && cartItem.Quantity > 1)
            {
                // Decrease the quantity by 1, ensuring it doesn't go below 1
                cartItem.Quantity -= 1;

                // Save changes to the database
                await context.SaveChangesAsync();

              
                var cartItemDto = new CartItemReadDto
                {
                    CartId = cartItem.CartId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    ProductId = cartItem.Product.Id,
                    Price = cartItem.Product.Price,
                    ProductName = cartItem.Product.ProductName,
                    ProductDescription = cartItem.Product.ProductDescription,
                    ImageUrl = cartItem.Product.ImageUrl
                };

                return cartItemDto;

            }

            // Return null or handle the case when the cart item doesn't exist or is at minimum quantity
            return null;

        }

       
    }
}
