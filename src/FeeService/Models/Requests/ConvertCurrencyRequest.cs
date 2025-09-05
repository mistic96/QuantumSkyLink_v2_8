using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class ConvertCurrencyRequest
{
    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string FromCurrency { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string ToCurrency { get; set; } = string.Empty;

    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    public DateTime? AtDate { get; set; }
}
