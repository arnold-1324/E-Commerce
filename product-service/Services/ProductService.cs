using MongoDB.Driver;
using ProductService.Models;
using ProductService.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IOptions<MongoDbSettings> settings)
        {
            if (settings?.Value == null)
                throw new ArgumentNullException(nameof(settings), "MongoDbSettings must be provided");

            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _products = database.GetCollection<Product>(settings.Value.ProductCollectionName);
        }

        public async Task<List<Product>> GetAsync()
        {
          //  return await _products.Find(_ => true).ToListAsync();
          
            // Return 100 random products using MongoDB aggregation $sample
            return await _products.Aggregate()
                .Sample(100)
                .ToListAsync();
    
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ProductId must be provided", nameof(id));

            var product = await _products.Find(p => p.ProductId == id).FirstOrDefaultAsync();
            if (product == null)
                throw new KeyNotFoundException($"No product found with id '{id}'");

            return product;
        }

        public async Task CreateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Example: ensure required fields
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required", nameof(product.Name));
            if (product.Price < 0)
                throw new ArgumentOutOfRangeException(nameof(product.Price), "Price must be non-negative");

            // If you want MongoDB to generate an ObjectId:
            if (string.IsNullOrWhiteSpace(product.ProductId))
                product.ProductId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

            await _products.InsertOneAsync(product);
        }

        public async Task UpdateAsync(string id, Product updated)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ProductId must be provided", nameof(id));
            if (updated == null)
                throw new ArgumentNullException(nameof(updated));

            // Keep the same id
            updated.ProductId = id;

            // Validate fields again
            if (string.IsNullOrWhiteSpace(updated.Name))
                throw new ArgumentException("Product name is required", nameof(updated.Name));
            if (updated.Price < 0)
                throw new ArgumentOutOfRangeException(nameof(updated.Price), "Price must be non-negative");

            var result = await _products.ReplaceOneAsync(p => p.ProductId == id, updated);
            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Cannot update; no product found with id '{id}'");
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ProductId must be provided", nameof(id));

            var result = await _products.DeleteOneAsync(p => p.ProductId == id);
            if (result.DeletedCount == 0)
                throw new KeyNotFoundException($"Cannot delete; no product found with id '{id}'");
        }
    }
}
