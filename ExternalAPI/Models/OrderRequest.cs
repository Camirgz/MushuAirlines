using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class OrderRequest
{
    [Required]
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; }

    [Required]
    [JsonPropertyName("flightGUID")]
    public string FlightGUID { get; set; }

    [Required]
    [JsonPropertyName("firstClass")]
    public bool? FirstClass { get; set; }

    [Required]
    [MaxLength(9, ErrorMessage = "The maximum number of passengers is 9.")]
    [MinLength(1, ErrorMessage = "At least one passenger is required.")]
    [JsonPropertyName("passengers")]
    public List<PassengerDTO> Passengers { get; set; }

    [Required]
    [JsonPropertyName("buyer")]
    public BuyerDTO Buyer { get; set; }

    [Required]
    [JsonPropertyName("payment")]
    public PaymentDTO Payment { get; set; }
}
