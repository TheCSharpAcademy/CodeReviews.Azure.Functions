using AzureFunctions.Helpers;
using Newtonsoft.Json;

namespace AzureFunctions.Models;

public class Order
{
    [JsonProperty("id")] public string? Id { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public StatusOptions Status { get; set; }
}