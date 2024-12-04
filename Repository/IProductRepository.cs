using EcommApp.Models.DTO;
using EcommApp.Models;

namespace EcommApp.Repository
{
    public interface IProductRepository
    {

        Task<Product> CreateProductAsync(CreateProductDto createProductDto);
        Task<IEnumerable<ProductAllDto>> GetAllProductsAsync();

        Task<bool> DeleteProductAsync(int productId);  // Add this line

        Task<Product> EditProductAsync(int productId, EditProductDto updatedProductDto);




    }
}
