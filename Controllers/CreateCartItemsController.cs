using EcommApp.Data;
using EcommApp.Exceptions;
using EcommApp.LiveData;
using EcommApp.Models.DTO;
using EcommApp.Repository;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace EcommApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateCartItemsController : ControllerBase
    {
        private readonly ICartItemRepository cartItemRepository;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> hubContext;
        private readonly EcommDbContext context;

        public CreateCartItemsController(
            ICartItemRepository cartItemRepository,
            Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> hubContext,
            EcommDbContext context
            )
        {
            this.cartItemRepository = cartItemRepository;
            this.hubContext = hubContext;
            this.context = context;
        }

        [HttpPost("AddItemToCart")]
        //[Authorize]
        public async Task<IActionResult> AddItemToCartAsync([FromBody] CreateCartDto createCartDto)
        {
            // Check if the item already exists in the cart
            var existingCartItem = await cartItemRepository.GetCartItemsByUserIdAsync(createCartDto.UserId);

            if (existingCartItem != null && existingCartItem.Any(c => c.ProductId == createCartDto.ProductId))
            {
                // If the item already exists in the cart, return a conflict response
                return Conflict(new { message = "Item already exists in the cart." });
            }

            // Item does not exist, add new item to cart
            var newCartItem = await cartItemRepository.AddItemToCartAsync(createCartDto);

            if (newCartItem == null)
            {
                // If adding the item failed, return a bad request response
                return BadRequest(new { message = "Failed to add item to cart." });
            }

                var product = await context.Products.FindAsync(newCartItem.ProductId);
                var cartItemDto = new CartItemReadDto
                {
                    CartId = newCartItem.CartId,
                    Quantity = newCartItem.Quantity,
                    UnitPrice = newCartItem.UnitPrice,
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price
                };

            var cartTotals = await cartItemRepository.GetCartTotalsAsync(createCartDto.UserId);
            
        // Send update only to the client who made the request
              await hubContext.Clients.Group(createCartDto.UserId).SendAsync("UpdateCartTotals", cartTotals);
              await hubContext.Clients.Group(createCartDto.UserId).SendAsync("CreateCartItemUpdate", cartItemDto);

            return CreatedAtRoute(null, new { cartId = newCartItem.CartId }, cartItemDto);           
        }


        [HttpGet("GetCartItemsByUser/{userId}")]
        [Authorize]
       public async Task<IActionResult> GetCartItemsByUserIdAsync(string userId)
        {
            var cartItemsList = await cartItemRepository.GetCartItemsByUserIdAsync(userId);

            if (cartItemsList == null || !cartItemsList.Any())
            {
                // If no cart items were found for the user, return a not found response
                return NotFound(new { message = "No cart items found for the user." });
            }

            // Transform cart items to CartItemReadDto list
            var cartItems = cartItemsList.Select(cartItem => new CartItemReadDto
            {
                CartId = cartItem.CartId,
                Quantity = cartItem.Quantity,
                UserId = cartItem.UserId,
                UnitPrice = cartItem.UnitPrice,
                // Map product details
                ProductId = cartItem.Product.Id,
                Price = cartItem.Product.Price,
                ProductName = cartItem.Product.ProductName,
                ProductDescription = cartItem.Product.ProductDescription,
                ImageUrl = cartItem.Product.ImageUrl
            }).ToList();

            // Return the DTOs in JSON format
            return Ok(cartItems);
        }



        [HttpDelete("RemoveAllItemsFromCart/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveAllItemsFromCartAsync(string userId)
        {
            var result = await cartItemRepository.RemoveAllItemsFromCartAsync(userId);

            if (result)
            {
                // If items were successfully removed, return an OK response
                return Ok(new { message = "All items removed from the cart successfully." });
            }

            // If no items were found to remove, return a not found response
            return NotFound(new { message = "No items found in the cart to remove." });
        }


        [HttpDelete("RemoveItemFromCart/{productId}/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveItemFromCartAsync(int productId, string userId)
        {
            var result = await cartItemRepository.RemoveItemFromCartAsync(productId, userId);


            if (result)
            {

                var cartTotals = await cartItemRepository.GetCartTotalsAsync(userId);

                // Broadcast the updated cart totals to the relevant users

                await hubContext.Clients.Group(userId).SendAsync("UpdateCartTotals", cartTotals);

                // If the item was successfully removed, return an OK response
                return Ok(new { message = "Item removed from the cart successfully." });
            }

            // If no matching item was found, return a not found response
            return NotFound(new { message = "Item not found in the cart." });
        }


        [HttpPut("UpdateCartItemQuantity/{productId}/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItemQuantityAsync( int productId, string userId, int newQuantity)
        {
            try
            {

                // Recalculate the cart totals after updating the item quantity
                var cartTotals = await cartItemRepository.GetCartTotalsAsync(userId);

                var updatedCartItemDto = await cartItemRepository.UpdateCartItemQuantityAsync(productId, userId, newQuantity);

                if (updatedCartItemDto != null)
                {

                    await hubContext.Clients.Group(userId).SendAsync("CartItemQuantityUpdated", updatedCartItemDto);

                    // Broadcast the updated cart totals to the specific user
                    await hubContext.Clients.Group(userId).SendAsync("UpdateCartTotals", cartTotals);

                    return Ok(new { message = "Cart item quantity updated successfully.", cartItem = updatedCartItemDto });
                }

                return NotFound(new { message = "Cart item not found." });
            }
            catch (StockLimitExceededException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }





        [HttpPut("DecreaseCartItemQuantity/{productId}/{userId}")]
        [Authorize]
        public async Task<IActionResult> DecreaseCartItemQuantityAsync(int productId, string userId, int newQuantity)
        {
            var updatedCartItem = await cartItemRepository.UpdateCartItemQuantityDeductAsync(productId, userId,  newQuantity);

            if (updatedCartItem != null)
            {

                var cartTotals = await cartItemRepository.GetCartTotalsAsync(userId);

                await hubContext.Clients.Group(userId).SendAsync("CartItemDeductedUpdated", updatedCartItem);

                await hubContext.Clients.Group(userId).SendAsync("UpdateCartTotals", cartTotals);

                // If the item's quantity was successfully updated, return an OK response
                return Ok(new { message = "Cart item quantity decreased successfully.", cartItem = updatedCartItem });
            }

            // If the cart item doesn't exist or is at minimum quantity, return a not found response
            return NotFound(new { message = "Unable to decrease quantity. Cart item not found or at minimum quantity." });
        }


        [HttpGet("GetCartItemsTotal/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetCartItemsTotalAsync(string userId)
        {
            var cartTotals = await cartItemRepository.GetCartTotalsAsync(userId);

            if (cartTotals == null)
            {
                // Handle the case where the cartTotals could not be calculated
                return NotFound(new { message = "Cart totals could not be calculated." });
            }

            // Notify all clients about the update
            await hubContext.Clients.Group(userId).SendAsync("UpdateCartTotals", cartTotals);

            // Return the totals in JSON format
            return Ok(cartTotals);
        }



    }
}
