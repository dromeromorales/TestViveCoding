using Microsoft.EntityFrameworkCore;
using ProductAPI.Domain.Entities;

namespace ProductAPI.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
                  
            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(1000);
                  
            entity.Property(e => e.Price)
                  .HasPrecision(18, 2);
                  
            entity.Property(e => e.Weight)
                  .HasPrecision(18, 2);
        });
    }
}