using HttpTrigger.Data;
using HttpTrigger.Models;

internal static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var initialProducts = new List<Product>
            {
                new Product { Id = "1", Name = "Turkish Delight", Price = 10.0m, InStock=14 },
                new Product { Id = "2", Name = "Turkish Coffee", Price = 20.0m, InStock=9 },
                new Product { Id = "3", Name = "Brazilian Coffee", Price = 25.0m, InStock=12 },
                new Product { Id = "4", Name = "Colombian Coffee", Price = 24.0m, InStock=14 },
                new Product { Id = "5", Name = "Ethiopian Coffee", Price = 24.0m, InStock=2 },
                new Product { Id = "6", Name = "Coffee seed Chocolate", Price = 24.0m, InStock=4 },
                new Product { Id = "7", Name = "Filter paper", Price = 4.25m, InStock=0 },
            };

        bool hasProducts = context.Products.AsEnumerable().Any();
        if (!hasProducts)
        {

            await context.Products.AddRangeAsync(initialProducts);
            await context.SaveChangesAsync();
        }

        var orderedProducts = new List<OrderedProduct>();
        var productIdsToAdd = new List<string> { "1", "3", "5" };

        var productsToAdd = initialProducts
            .Where(p => productIdsToAdd.Contains(p.Id))
            .Select(p => new OrderedProduct
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Count = new Random().Next(3),
            });

        orderedProducts.AddRange(productsToAdd);

        var order = new Order
        {
            Id = "1",
            Products = orderedProducts,
            CustomerEmail = "serdar415@gmail.com",
            CreateDate = DateTime.Now
        };



        bool hasOrder = context.Orders.AsEnumerable().Any();
        if (!hasOrder)
        {
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
        }
    }
}

