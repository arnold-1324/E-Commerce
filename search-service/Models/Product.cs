using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("ProductId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public double Price { get; set; }

    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Subcategory")]
    public string Subcategory { get; set; } = string.Empty;

    [JsonPropertyName("Attributes")]
    public Dictionary<string, string> Attributes { get; set; } = new();

    [JsonPropertyName("Stock")]
    public int Stock { get; set; }

    [JsonPropertyName("Brand")]
    public string Brand { get; set; } = string.Empty;

    [JsonPropertyName("Rating")]
    public double Rating { get; set; }

    [JsonPropertyName("Tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("RelatedProducts")]
    public List<string> RelatedProducts { get; set; } = new();

    [JsonPropertyName("ImageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
}
