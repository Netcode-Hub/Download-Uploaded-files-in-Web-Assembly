using Microsoft.EntityFrameworkCore;
using ORMExplained.Server.Models;

namespace ORMExplained.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<UploadResult> Uploads { get; set; }
    }
}
