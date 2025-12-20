using Microsoft.EntityFrameworkCore;
using WebAPIJwtAuth.Domain.Entities;
using WebAPIJwtAuth.Infrastructure.Data;
using WebAPIJwtAuth.Infrastructure.Repositories.Interfaces;

namespace WebAPIJwtAuth.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        public async Task<IEnumerable<Product>> GetProductsWithDetailsAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            Guid? categoryId = null,
            Guid? brandId = null,
            bool? isFeatured = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = "name",
            bool sortAsc = true)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description!.Contains(search) ||
                    p.SKU.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == isFeatured.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "price" => sortAsc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "created" => sortAsc ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                "rating" => sortAsc ? query.OrderBy(p => p.AverageRating) : query.OrderByDescending(p => p.AverageRating),
                "name" => sortAsc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };

            // Apply pagination
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int count = 5)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return new List<Product>();

            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive &&
                           p.Id != productId &&
                           (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null)
        {
            var query = _dbSet.Where(p => p.SKU == sku && p.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}