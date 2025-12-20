using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Repositories.Interfaces
{
    // JWTAuth.Infrastructure/Repositories/IProductReviewRepository.cs
    public interface IProductReviewRepository : IGenericRepository<ProductReview>
    {
        Task<IEnumerable<ProductReview>> GetReviewsByProductIdAsync(Guid productId, int page = 1, int pageSize = 10);
        Task<ProductReview?> GetReviewWithDetailsAsync(Guid reviewId);
        Task<double?> GetAverageRatingAsync(Guid productId);
        Task<bool> HasUserReviewedAsync(Guid productId, Guid userId);
    }
}
