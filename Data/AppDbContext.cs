using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App_CCP.Models;
using Microsoft.AspNetCore.Identity;

namespace App_CCP.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public DbSet<Partners> Partners { get; set; } // Adaugă DbSet pentru parteneri
        public DbSet<Review> Reviews { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurarea entitatii Users
            builder.Entity<Users>(entity =>
    {
        entity.Property(u => u.FullName)
              .HasMaxLength(100)
              .IsRequired(false); // Permite valori NULL

        entity.Property(u => u.Address)
              .HasMaxLength(255)
              .IsRequired(false);

        entity.Property(u => u.PlaceOfBirth)
              .HasMaxLength(100)
              .IsRequired(false);

    });

            // Seed pentru roluri
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
            );
            // Configurarea entității Partners
            builder.Entity<Partners>(entity =>
            {
                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Description)
                      .HasMaxLength(255)
                      .IsRequired(false); // Permite valori NULL

                entity.Property(p => p.DiscountDetails)
                      .HasMaxLength(255)
                      .IsRequired(false);

                entity.Property(p => p.LogoUrl)
                      .HasMaxLength(255)
                      .IsRequired(false);

                entity.Property(p => p.WebsiteUrl)
                      .HasMaxLength(100)
                      .IsRequired(false);
            });
        }
    }
}