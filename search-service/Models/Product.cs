using System.Collections.Generic;

namespace SearchService.Models
{
    public record Product(
        string Id,
        string Name,
        string Description,
        decimal Price,
        List<string> Tags
    );
}
