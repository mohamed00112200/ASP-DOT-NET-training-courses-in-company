using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data;

public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options)
{
    private static readonly DateTime SeedTimestamp =
        new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(category =>
        {
            category.HasKey(item => item.Id);
            category.Property(item => item.Name).HasMaxLength(100).IsRequired();
            category.HasQueryFilter(item => !item.IsDeleted);
        });

        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(item => item.Id);
            product.Property(item => item.Name).HasMaxLength(200).IsRequired();
            product.Property(item => item.ConcurrencyToken).IsConcurrencyToken();
            product.HasIndex(item => item.Name);
            product.HasQueryFilter(item => !item.IsDeleted);
            product
                .HasOne(item => item.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Electronics",
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            },
            new Category
            {
                Id = 2,
                Name = "Books",
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 25000m,
                CategoryId = 1,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                ConcurrencyToken = Guid.Parse("10000000-0000-0000-0000-000000000001")
            },
            new Product
            {
                Id = 2,
                Name = "Keyboard",
                Price = 1200m,
                CategoryId = 1,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                ConcurrencyToken = Guid.Parse("10000000-0000-0000-0000-000000000002")
            },
            new Product
            {
                Id = 3,
                Name = "Smartphone",
                Price = 18000m,
                CategoryId = 1,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                ConcurrencyToken = Guid.Parse("10000000-0000-0000-0000-000000000003")
            },
            new Product
            {
                Id = 4,
                Name = "Novel",
                Price = 350m,
                CategoryId = 2,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                ConcurrencyToken = Guid.Parse("10000000-0000-0000-0000-000000000004")
            });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDelete();
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(item => item.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = now <= entry.Entity.UpdatedAt
                    ? entry.Entity.UpdatedAt.AddTicks(1)
                    : now;
            }
        }
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<Product>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.ConcurrencyToken = Guid.NewGuid();
        }
    }
}
