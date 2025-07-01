using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchService.Models
{
    public record Product
    {
        [JsonPropertyName("ProductId")]
        public string ProductId { get; init; } = string.Empty;

        [JsonPropertyName("Name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("Price")]
        public double Price { get; init; }

        [JsonPropertyName("Category")]
        public string Category { get; init; } = string.Empty;

        [JsonPropertyName("Subcategory")]
        public string Subcategory { get; init; } = string.Empty;

        [JsonPropertyName("Attributes")]
        public Dictionary<string, string> Attributes { get; init; } = new();

        [JsonPropertyName("Stock")]
        public int Stock { get; init; }

        [JsonPropertyName("Brand")]
        public string Brand { get; init; } = string.Empty;

        [JsonPropertyName("Rating")]
        public double Rating { get; init; }

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; init; } = new();

        [JsonPropertyName("RelatedProducts")]
        public List<string> RelatedProducts { get; init; } = new();

        [JsonPropertyName("ImageUrl")]
        public string ImageUrl { get; init; } = string.Empty;
    }
}
