using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchService.Services
{

    public class ProductIndexService : IProductIndexService
    {
        public readonly List<(double Price, string ProductId)> _productIndex = new();
        public readonly object _lock = new();
        private readonly Microsoft.Extensions.Logging.ILogger<ProductIndexService> _logger;

        public ProductIndexService(Microsoft.Extensions.Logging.ILogger<ProductIndexService> logger)
        {
            _logger = logger;
        }

        public void AddProduct(double price, string productId)
        {
            lock (_lock)
            {
                _productIndex.Add((price, productId));
                _productIndex.Sort((a, b) => a.Price.CompareTo(b.Price));
            }
        }

        public List<string> GetProductIdsInPriceRange(double minPrice, double maxPrice)
        {
        lock (_lock)
        {
            _logger.LogInformation("Filtering products in price range: {MinPrice} - {MaxPrice}. Total products: {Count}", minPrice, maxPrice, _productIndex.Count);
            int left = LowerBound(minPrice);
            int right = UpperBound(maxPrice);

            var filtered = _productIndex
                .Skip(left)
                .Take(right - left + 1)
                .Select(p => p.ProductId)
                .ToList();

            _logger.LogInformation("Filtered products count: {FilteredCount}. ProductIds: {ProductIds}", filtered.Count, string.Join(",", filtered));
            return filtered;
        }
        }

        private int LowerBound(double target)
        {
            int low = 0, high = _productIndex.Count - 1, ans = _productIndex.Count;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (_productIndex[mid].Price >= target)
                {
                    ans = mid;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }
            return ans;
        }

        private int UpperBound(double target)
        {
            int low = 0, high = _productIndex.Count - 1, ans = -1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (_productIndex[mid].Price <= target)
                {
                    ans = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return ans;
        }
    }
 }