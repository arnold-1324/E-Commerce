using System;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Services
{
    public interface IProductIndexService
    {
        void AddProduct(double price, string productId);
        List<string> GetProductIdsInPriceRange(double minPrice, double maxPrice);
    }
}