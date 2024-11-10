using System.Text.Json.Serialization;

namespace TimerFunction.Models;


internal class Order
{
    public string Id { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public ICollection<OrderedProduct> Products { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Status Status { get; set; } = Status.OrderPlaced;
    public decimal TotalFee
    {
        get
        {
            return Products.Sum(p => p.Price * p.Count);
        }
    }
}

public enum Status
{
    OrderPlaced,
    PaymentComplete,
    OrderShipped,
    ShipmentFulfilled
}