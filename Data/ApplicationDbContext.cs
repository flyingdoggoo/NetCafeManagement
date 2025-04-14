using Microsoft.EntityFrameworkCore;

namespace QuanLyNet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<QuanLyNet.Models.User> Users { get; set; }
        public DbSet<QuanLyNet.Models.Computer> Computers { get; set; }
        public DbSet<QuanLyNet.Models.Bill> Bills { get; set; }
    }
}
