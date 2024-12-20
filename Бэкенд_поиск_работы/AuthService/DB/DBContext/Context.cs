using AuthService.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.DB.DBContext
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
