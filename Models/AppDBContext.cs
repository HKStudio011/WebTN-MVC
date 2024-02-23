using WebTN_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebTN_MVC.Models
{
    public class AppDBContext : IdentityDbContext<AppUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Contact.Contact> Contacts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = item.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    item.SetTableName(tableName.Substring(6));// or Replace("AspNet","")
                }
            }
        }
    }
}