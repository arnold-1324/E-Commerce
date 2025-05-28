using System.Collections.Generic;
using ProductService.Models;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAsync();

        Task<Product> GetByIdAsync(string id);

        Task CreateAsync(Product product);

        Task UpdateAsync(string id, Product productIn); 

        Task DeleteAsync(string id);
           
    }
}
