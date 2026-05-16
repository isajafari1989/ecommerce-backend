using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUser(modelBuilder);
            ConfigureProduct(modelBuilder);
            ConfigureRefreshToken(modelBuilder);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasMaxLength(50)
                    .HasDefaultValue("Customer");

                entity.Property(u => u.CreatedAt)
                    .IsRequired();

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Products)
                    .WithOne(p => p.Seller)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureProduct(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Description)
                    .HasMaxLength(2000);

                entity.Property(p => p.SKU)
                    .HasMaxLength(100);

                entity.HasIndex(p => p.SKU)
                    .IsUnique();

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.Property(p => p.Stock)
                    .IsRequired();

                entity.Property(p => p.ImageUrl)
                    .HasMaxLength(500);

                entity.Property(p => p.IsActive)
                    .HasDefaultValue(true);

                entity.Property(p => p.CreatedAt)
                    .IsRequired();

                entity.Property(p => p.UpdatedAt)
                    .IsRequired(false);
            });
        }

        private void ConfigureRefreshToken(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(rt => rt.Token)
                    .IsUnique();

                entity.Property(rt => rt.CreatedAt)
                    .IsRequired();

                entity.Property(rt => rt.Expires)
                    .IsRequired();

                entity.Property(rt => rt.ReasonRevoked)
                    .HasMaxLength(200);
            });
        }
    }
