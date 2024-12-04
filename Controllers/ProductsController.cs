using EcommApp.Models.DTO;
using EcommApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the image file
            var imageValidationResult = ValidateImageFile(createProductDto.ImageFile);
            if (!string.IsNullOrEmpty(imageValidationResult))
            {
                return BadRequest(imageValidationResult);
            }

            try
            {

                var product = await _productRepository.CreateProductAsync(createProductDto);
                // Adjust according to your GetProduct method


                var productResponse = new CreateProductResponseDto
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    Price = product.Price,
                    UnitsAvailable = product.UnitsAvailable,
                    ImageUrl = product.ImageUrl
                };

                return Ok(productResponse);

            }
            catch (Exception ex)
            {
                // Log the exception and handle as necessary
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private string ValidateImageFile(IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxFileSize = 5 * 1024 * 1024; // 5MB

            if (imageFile != null)
            {
                if (!allowedExtensions.Any(ext => imageFile.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    return "Bad file type error. Only accepts JPG, PNG, JPEG.";
                }

                if (imageFile.Length > maxFileSize)
                {
                    return "Size limit of 5MB exceeded.";
                }
            }

            return null;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products);
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var deleteResult = await _productRepository.DeleteProductAsync(id);

                if (!deleteResult)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." });
                }

                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }



        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditProduct(int id, [FromForm] EditProductDto updatedProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)) });
            }

            // Validate the image file if it's being updated
            if (updatedProductDto.ImageFile != null)
            {
                var imageValidationResult = ValidateImageFile(updatedProductDto.ImageFile);
                if (!string.IsNullOrEmpty(imageValidationResult))
                {
                    return BadRequest(new { message = imageValidationResult });
                }
            }

            try
            {
                var updatedProduct = await _productRepository.EditProductAsync(id, updatedProductDto);

                if (updatedProduct == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." });
                }

                // Create a response DTO
                var productResponse = new CreateProductResponseDto
                {
                    Id = updatedProduct.Id,
                    ProductName = updatedProduct.ProductName,
                    ProductDescription = updatedProduct.ProductDescription,
                    Price = updatedProduct.Price,
                    UnitsAvailable = updatedProduct.UnitsAvailable,
                    ImageUrl = updatedProduct.ImageUrl
                };

                return Ok(new { message = "Product updated successfully.", product = productResponse });
            }
            catch (Exception ex)
            {
                // Log the exception and handle as necessary
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }


    }
}
