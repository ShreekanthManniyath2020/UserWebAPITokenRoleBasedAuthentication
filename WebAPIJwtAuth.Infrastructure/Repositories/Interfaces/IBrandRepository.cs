using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Repositories.Interfaces
{
    // JWTAuth.Infrastructure/Repositories/IBrandRepository.cs
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
        Task<Brand?> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    }
}
