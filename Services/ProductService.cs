using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Data;
using Shared.Models.DTOs;
using Shared.Models.Entities;

namespace ProductManagement.API.Services
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetProductsAsync(ProductSearchFilter filter);
        Task<Product> CreateProductAsync(CreateProductRequest request, Guid userId);
        Task<Product> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid userId);
        Task<bool> DeleteProductAsync(Guid id, Guid userId);
        Task<bool> SoftDeleteUserProductsAsync(Guid userId);
        Task<bool> RestoreUserProductsAsync(Guid userId);
    }

    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(ProductSearchFilter filter)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(p => p.Name.Contains(filter.Name));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (filter.IsAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == filter.IsAvailable.Value);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(p => p.UserId == filter.UserId.Value);
            }

            // Пагинация
            query = query.Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize);

            return await query.ToListAsync();
        }

        public async Task<Product> CreateProductAsync(CreateProductRequest request, Guid userId)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid userId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }

            // Проверяем, что пользователь является владельцем продукта
            if (product.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only edit your own products");
            }

            // Обновляем только переданные поля
            if (!string.IsNullOrEmpty(request.Name))
            {
                product.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                product.Description = request.Description;
            }

            if (request.Price.HasValue)
            {
                product.Price = request.Price.Value;
            }

            if (request.IsAvailable.HasValue)
            {
                product.IsAvailable = request.IsAvailable.Value;
            }

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(Guid id, Guid userId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false;
            }

            // Проверяем, что пользователь является владельцем продукта
            if (product.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own products");
            }

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserProductsAsync(Guid userId)
        {
            var products = await _context.Products
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreUserProductsAsync(Guid userId)
        {
            var products = await _context.Products
                .Where(p => p.UserId == userId && p.IsDeleted)
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsDeleted = false;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
