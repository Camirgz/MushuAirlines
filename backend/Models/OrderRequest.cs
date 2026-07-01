using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models;

public class BuyerDTO
{
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [JsonPropertyName("nationality")]
    public string Nationality { get; set; }

    [Required]
    [MaxLength(255)]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(255)]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("lastName2")]
    public string LastName2 { get; set; }

    [Required]
    [MaxLength(20)]
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class PassengerDTO
{
    [Required]
    [JsonPropertyName("carryOn")]
    public bool? CarryOn { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    [JsonPropertyName("checked")]
    public int? CheckedBaggage { get; set; }

    [Required]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "Passport must be exactly 9 characters.")]
    [JsonPropertyName("passport")]
    public string Passport { get; set; }

    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be YYYY-MM-DD")]
    [JsonPropertyName("passportExpirationDate")]
    public string PassportExpirationDate { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Country must be ISO 3166 alpha 3 (3 chars).")]
    [JsonPropertyName("passportCountry")]
    public string PassportCountry { get; set; }

    [Required]
    [MaxLength(255)]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(255)]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("lastName2")]
    public string LastName2 { get; set; }

    [Required]
    [RegularExpression("^[MFO]$", ErrorMessage = "Gender must be M, F, or O.")]
    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be YYYY-MM-DD")]
    [JsonPropertyName("birthDate")]
    public string BirthDate { get; set; }
}

public class PaymentDTO
{
    public string PaymentMethod { get; set; };

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
