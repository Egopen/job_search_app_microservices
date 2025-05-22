using EmployerService.Infrastructure.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployerService.Infrastructure.DB.DBContext
{
    public class Context : DbContext
    {
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Experience> Experiences { get; set; } = null!;
        public DbSet<Vacancy> Vacancies { get; set; } = null!;
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
                        IF NOT EXISTS (SELECT 1 FROM Experience WHERE Id = 1)
                        BEGIN
                            INSERT INTO Experience (Id, Desc) VALUES (1, 'Без опыта');
                        END;
                        IF NOT EXISTS (SELECT 1 FROM Experience WHERE Id = 2)
                        BEGIN
                            INSERT INTO Experience (Id, Desc) VALUES (2, '1-3 года');
                        END;
                        IF NOT EXISTS (SELECT 1 FROM Experience WHERE Id = 3)
                        BEGIN
                            INSERT INTO Experience (Id, Desc) VALUES (3, '3-6 лет');
                        END;
                    ";
                    command.ExecuteNonQuery();

                    command.CommandText = @"
                        IF NOT EXISTS (SELECT 1 FROM Status WHERE Id = 1)
                        BEGIN
                            INSERT INTO Status (Id, Desc, IsActive) VALUES (1, 'В поиске', 1);
                        END;
                        IF NOT EXISTS (SELECT 1 FROM Status WHERE Id = 2)
                        BEGIN
                            INSERT INTO Status (Id, Desc, IsActive) VALUES (2, 'Не ищу', 0);
                        END;
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
