using System;
using System.Threading.Tasks;

namespace SearchService.Services
{
	public interface ISearchCache
	{
		Task<string?> GetCachedResultAsync(string query);
		Task SetCachedResultAsync(string query, string resultJson, TimeSpan expiration);
	}
}
