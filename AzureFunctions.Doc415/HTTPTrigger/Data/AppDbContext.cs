using HttpTrigger.Models;
using Microsoft.EntityFrameworkCore;


namespace HttpTrigger.Data;

internal class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .ToContainer("Products")
            .HasPartitionKey(e => e.Id);



        modelBuilder.Entity<Order>()
            .ToContainer("Orders")
            .HasPartitionKey(e => e.Id);

        modelBuilder.Entity<Order>().OwnsMany(o => o.Products);

    }
}
