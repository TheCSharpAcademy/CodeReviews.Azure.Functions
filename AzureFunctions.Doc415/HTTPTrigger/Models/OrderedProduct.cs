namespace HttpTrigger.Models;

internal class OrderedProduct
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
}
