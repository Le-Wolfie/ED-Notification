using System.ComponentModel.DataAnnotations;

namespace EventNotify.DTOs;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "OrderId must be a positive number")]
    public int OrderId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}
