using System;
using System.Text.Json.Serialization;

namespace SearchService.Models
{
    public class ProductEvent
    {
        [JsonPropertyName("EventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("Product")]
        public Product? Product { get; set; }

        [JsonPropertyName("Timestamp")]
        public DateTime? Timestamp { get; set; }
    }
}
