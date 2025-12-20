using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByResetTokenAsync(string resetToken);
        Task<bool> UserExistsAsync(string email, string username);
    }

}
