using Xunit;
using System.Collections.Generic;
using SearchService.Utils;
namespace SearchService.Tests
{
    public class TrieTests
    {
        [Fact]
        public void InsertAndSearch_ReturnsCorrectIds()
        {
            var trie = new Trie();
            trie.Insert("apple", "p1");
            trie.Insert("app", "p2");

            List<string> result = trie.Search("app");
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }

        [Fact]
        public void Search_NonexistentPrefix_ReturnsEmpty()
        {
            var trie = new Trie();
            trie.Insert("banana", "p3");

            var result = trie.Search("app");
            Assert.Empty(result);
        }
    }
}
