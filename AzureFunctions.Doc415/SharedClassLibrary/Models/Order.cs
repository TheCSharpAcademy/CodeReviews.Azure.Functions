using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedClassLibrary.Models;

public class Order
{
    public string Id { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public ICollection<OrderedProduct> Products { get; set; }
    public Status Status { get; set; } = Status.OrderPlaced;
    public Status PreviousStatus { get; set; } = Status.OrderPlaced;

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