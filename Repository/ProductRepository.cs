using EcommApp.Data;
using EcommApp.Models;
using EcommApp.Models.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace EcommApp.Repository
{
    public class ProductRepository : IProductRepository
    {


        private readonly EcommDbContext _context;
        // Dependency for file storage, if needed
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductRepository(EcommDbContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Product> CreateProductAsync(CreateProductDto createProductDto)
        {

            // Handle the image file and get the path
            var imagePath = await SaveImageAsync(createProductDto.ImageFile);


            var product = new Product
            {
                ProductName = createProductDto.ProductName,
                ProductDescription = createProductDto.ProductDescription,
                Price = createProductDto.Price,
                UnitsAvailable = createProductDto.UnitsAvailable,
                ImageUrl = imagePath
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }



        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var localFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", fileName);

                using (var stream = new FileStream(localFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/Images/{fileName}";

                return urlFilePath;
            }

            return null;
        }


        public async Task<IEnumerable<ProductAllDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                                         .OrderByDescending(p => p.Id) // Order by Id in descending order
                                         .ToListAsync();

            return products.Select(p => new ProductAllDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                Price = p.Price,
                UnitsAvailable = p.UnitsAvailable,
                ImageUrl = p.ImageUrl
            }).ToList();
        }

        public  async Task<bool> DeleteProductAsync(int productId)
        {
            // Find the product by ID
            var product = await _context.Products.FindAsync(productId);

            // If no product found, return false
            if (product == null)
            {
                return false;
            }

            // Remove the product from the database
            _context.Products.Remove(product);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return true indicating successful deletion
            return true;

        }

        public async Task<Product> EditProductAsync(int productId, EditProductDto updatedProductDto)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return null; // Or throw an exception if preferred
            }

            // Update properties if they are provided in the DTO
            if (!string.IsNullOrEmpty(updatedProductDto.ProductName))
            {
                product.ProductName = updatedProductDto.ProductName;
            }

            if (!string.IsNullOrEmpty(updatedProductDto.ProductDescription))
            {
                product.ProductDescription = updatedProductDto.ProductDescription;
            }

            if (updatedProductDto.Price != default)
            {
                product.Price = (decimal)updatedProductDto.Price;
            }

            if (updatedProductDto.UnitsAvailable != default)
            {
                product.UnitsAvailable = (int)updatedProductDto.UnitsAvailable;
            }

            if (updatedProductDto.ImageFile != null)
            {
                var imagePath = await SaveImageAsync(updatedProductDto.ImageFile);
                product.ImageUrl = imagePath;
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return product;
        }
    }
}
