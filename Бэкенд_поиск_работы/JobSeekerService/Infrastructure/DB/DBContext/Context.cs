using JobSeekerService.Infrastructure.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace JobSeekerService.Infrastructure.DB.DBContext
{
    public class Context : DbContext
    {
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<Experience> Experience { get; set; } = null!;
        public DbSet<Response> Responses { get; set; } = null!;
        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureCreated();
            InitializeData();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        private void InitializeData()
        {
            using (var connection = Database.GetDbConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        IF NOT EXISTS (SELECT 1 FROM Status WHERE Id = 1)
                        BEGIN
                            INSERT INTO Status (Id, Desc, Is_active) VALUES (1, 'В поиске', 1);
                        END;
                        IF NOT EXISTS (SELECT 1 FROM Status WHERE Id = 2)
                        BEGIN
                            INSERT INTO Status (Id, Desc, Is_active) VALUES (2, 'Не ищу', 0);
                        END;
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
