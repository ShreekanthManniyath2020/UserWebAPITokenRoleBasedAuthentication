using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetByIdWithDetailsAsync(Guid id);
        Task<Product?> GetBySkuAsync(string sku);
        Task<IEnumerable<Product>> GetProductsWithDetailsAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            Guid? categoryId = null,
            Guid? brandId = null,
            bool? isFeatured = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = "name",
            bool sortAsc = true);

        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int count = 5);
        Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null);
    }
}
