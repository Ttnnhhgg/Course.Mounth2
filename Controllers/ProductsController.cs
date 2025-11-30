using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Services;
using Shared.Models.DTOs;
using Shared.Models.Entities;
using System.Security.Claims;

namespace ProductManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductSearchFilter filter)
        {
            try
            {
                var products = await _productService.GetProductsAsync(filter);
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    UserId = p.UserId,
                    CreatedAt = p.CreatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    UserId = product.UserId,
                    CreatedAt = product.CreatedAt
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var product = await _productService.CreateProductAsync(request, userId);

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    UserId = product.UserId,
                    CreatedAt = product.CreatedAt
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var product = await _productService.UpdateProductAsync(id, request, userId);

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    UserId = product.UserId,
                    CreatedAt = product.CreatedAt
                };

                return Ok(productDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _productService.DeleteProductAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { message = "Product not found" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("admin/deactivate-user-products/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUserProducts(Guid userId)
        {
            try
            {
                await _productService.SoftDeleteUserProductsAsync(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user products for user ID: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("admin/activate-user-products/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUserProducts(Guid userId)
        {
            try
            {
                await _productService.RestoreUserProductsAsync(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user products for user ID: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }
    }
}
