using Microsoft.EntityFrameworkCore;
using WebAPIJwtAuth.Domain.Entities;
using WebAPIJwtAuth.Infrastructure.Data;
using WebAPIJwtAuth.Infrastructure.Repositories.Interfaces;

namespace WebAPIJwtAuth.Infrastructure.Repositories
{
    public class ProductReviewRepository : GenericRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductReview>> GetReviewsByProductIdAsync(Guid productId, int page = 1, int pageSize = 10)
        {
            return await _dbSet
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<ProductReview?> GetReviewWithDetailsAsync(Guid reviewId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<double?> GetAverageRatingAsync(Guid productId)
        {
            return await _dbSet
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.Rating);
        }

        public async Task<bool> HasUserReviewedAsync(Guid productId, Guid userId)
        {
            return await _dbSet
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        }
    }
}