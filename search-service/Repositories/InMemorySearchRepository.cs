using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public class InMemorySearchRepository : ISearchRepository
    {
        private readonly ConcurrentDictionary<string, Product> _store = new();

        public Task AddOrUpdateAsync(Product product)
        {
            _store.AddOrUpdate(product.ProductId, product, (_, __) => product);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var result = ids
                .Where(id => _store.TryGetValue(id, out _))
                .Select(id => _store[id])
                .ToList();
            return Task.FromResult((IReadOnlyList<Product>)result);
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(_store.Count);
        }
    }
}
