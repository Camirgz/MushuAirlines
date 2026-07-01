using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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