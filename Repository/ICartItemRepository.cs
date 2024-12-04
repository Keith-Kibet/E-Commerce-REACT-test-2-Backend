using EcommApp.Models.DTO;
using EcommApp.Models;

namespace EcommApp.Repository
{
    public interface ICartItemRepository
    {

        Task<Cart> AddItemToCartAsync(CreateCartDto createCartDto);
        Task<bool> RemoveItemFromCartAsync(int productId, string userId);
        Task<IEnumerable<Cart>> GetCartItemsByUserIdAsync(string userId);

        Task<CartItemReadDto> UpdateCartItemQuantityAsync(int productId, string userId, int newQuantity);

        Task<CartItemReadDto> UpdateCartItemQuantityDeductAsync(int productId, string userId, int newQuantity);



        // Method to remove all items from the cart for a specific user
        Task<bool> RemoveAllItemsFromCartAsync(string userId);

        Task<CartItemsSummation> GetCartTotalsAsync(string userId);


    }
}
