using Microsoft.EntityFrameworkCore;
using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}