// src/Models/Product.cs
using System.Collections.Generic;
using Nest;

namespace SearchService.Models
{
    public class Product
    {
        [Keyword(Name = "product_id")]
        public string ProductId { get; set; } = string.Empty;

        [Text(Name = "name")]
        public string Name { get; set; } = string.Empty;

        [Text(Name = "description")]
        public string Description { get; set; } = string.Empty;

        // Fully qualify Nest.NumberType so it never tries to bind to System.Type
        [Number(Name = "price")]
        public double Price { get; set; }

        [Keyword(Name = "category")]
        public string Category { get; set; } = string.Empty;

        [Keyword(Name = "subcategory")]
        public string Subcategory { get; set; } = string.Empty;

        [Object(Name = "attributes")]
        public Dictionary<string, string> Attributes { get; set; } = new();

        [Number(Name = "stock")]
        public int Stock { get; set; }

        [Keyword(Name = "brand")]
        public string Brand { get; set; } = string.Empty;

        [Number(Name = "rating")]
        public double Rating { get; set; }

        [Keyword(Name = "tags")]
        public List<string> Tags { get; set; } = new();

        [Keyword(Name = "related_products")]
        public List<string> RelatedProducts { get; set; } = new();

        [Text(Name = "image_url")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
