using System.Collections.Generic;

namespace SearchService.Models
{ 
 public record SearchResult
    {
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int Size { get; init; }
        public IReadOnlyList<Product> Items { get; init; } = new List<Product>();
    }

}
