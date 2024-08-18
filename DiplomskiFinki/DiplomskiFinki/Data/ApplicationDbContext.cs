using DiplomskiFinki.Models;
using DiplomskiFinki.Models.Dto;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiFinki.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Diploma> Diplomas { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<DiplomaStatus> DiplomaStatuses { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data if it doesn't already exist
            modelBuilder.Entity<Step>().HasData(
                new Step { SubStep = 1, SubStepName = "Пријава" },
                new Step { SubStep = 2, SubStepName = "Прифаќање на темата од студентот" },
                new Step { SubStep = 3, SubStepName = "Валидирање од службата за студентски прашања" },
                new Step { SubStep = 3.1, SubStepName = "Одобрение од продекан за настава" },
                new Step { SubStep = 4, SubStepName = "Одобрение за оценка од ментор" },
                new Step { SubStep = 5, SubStepName = "Забелешки од комисија" },
                new Step { SubStep = 6, SubStepName = "Валидирање на услови за одбрана" },
                new Step { SubStep = 7, SubStepName = "Одбрана" },
                new Step { SubStep = 8, SubStepName = "Архива" }
            );
        }
    }
}
