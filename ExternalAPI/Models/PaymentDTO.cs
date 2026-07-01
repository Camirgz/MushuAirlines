using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExternalAPI.Models;

public class PaymentDTO
{
    [Required]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits without spaces.")]
    [JsonPropertyName("cardNumber")]
    public string CardNumber { get; set; }

    [Required]
    [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Expiration must be YYYY-MM")]
    [JsonPropertyName("cardExpiration")]
    public string CardExpiration { get; set; }

    [Required]
    [RegularExpression(@"^\d{4,}$", ErrorMessage = "CVV must be at least 4 digits.")]
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }

    [Required]
    [MaxLength(256)]
    [JsonPropertyName("cardHolderName")]
    public string CardHolderName { get; set; }
}