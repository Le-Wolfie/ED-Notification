namespace EventNotify.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";  // Pending, Completed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
