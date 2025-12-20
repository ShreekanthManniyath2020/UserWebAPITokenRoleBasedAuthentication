using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Repositories.Interfaces
{
    // JWTAuth.Infrastructure/Repositories/ICategoryRepository.cs
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    }
}
