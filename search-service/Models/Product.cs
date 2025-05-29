using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchService.Models
{
    public record Product
    {
        [JsonPropertyName("_id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("product_id")]
        public string ProductId { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("price")]
        public double Price { get; init; }

        [JsonPropertyName("category")]
        public string Category { get; init; } = string.Empty;

        [JsonPropertyName("subcategory")]
        public string Subcategory { get; init; } = string.Empty;

        [JsonPropertyName("attributes")]
        public Dictionary<string, string> Attributes { get; init; } = new();

        [JsonPropertyName("stock")]
        public int Stock { get; init; }

        [JsonPropertyName("brand")]
        public string Brand { get; init; } = string.Empty;

        [JsonPropertyName("rating")]
        public double Rating { get; init; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; init; } = new();

        [JsonPropertyName("related_products")]
        public List<string> RelatedProducts { get; init; } = new();

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; init; } = string.Empty;
    }
}
