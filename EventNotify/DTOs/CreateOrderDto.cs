using System.ComponentModel.DataAnnotations;

namespace EventNotify.DTOs;

public class CreateOrderDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive number")]
    public int UserId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
    public decimal Total { get; set; }
}
