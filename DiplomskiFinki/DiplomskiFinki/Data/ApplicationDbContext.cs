using DiplomskiFinki.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiFinki.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Diploma> Diplomas { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
