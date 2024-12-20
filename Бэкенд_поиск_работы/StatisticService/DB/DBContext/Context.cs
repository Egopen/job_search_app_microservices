using Microsoft.EntityFrameworkCore;
using StatisticService.DB.Models;

namespace StatisticService.DB.DBContext
{
    public class Context : DbContext
    {
        public DbSet<VacancyStatistic> VacancyStatistics { get; set; } = null!;
        public DbSet<ResumeStatistic> ResumeStatistics { get; set; } = null!;
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
