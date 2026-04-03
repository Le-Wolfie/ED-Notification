namespace EventNotify.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";  // Pending, Success, Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
