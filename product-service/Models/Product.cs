using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ProductService.Models
{
	[BsonIgnoreExtraElements]
	public class Product
	{
		[BsonElement("product_id")]
		public string ProductId { get; set; } = string.Empty;

		[BsonElement("name")]
		public string Name { get; set; } = string.Empty;

		[BsonElement("description")]
		public string Description { get; set; } = string.Empty;

		[BsonElement("price")]
		public double Price { get; set; }

		[BsonElement("category")]
		public string Category { get; set; } = string.Empty;

		[BsonElement("subcategory")]
		public string Subcategory { get; set; } = string.Empty;

		[BsonElement("attributes")]
		public Dictionary<string, string> Attributes { get; set; } = new();

		[BsonElement("stock")]
		public int Stock { get; set; }

		[BsonElement("brand")]
		public string Brand { get; set; } = string.Empty;

		[BsonElement("rating")]
		public double Rating { get; set; }

		[BsonElement("tags")]
		public List<string> Tags { get; set; } = new();

		[BsonElement("related_products")]
		public List<string> RelatedProducts { get; set; } = new();

		[BsonElement("image_url")]
		public string ImageUrl { get; set; } = string.Empty;
	}
}
