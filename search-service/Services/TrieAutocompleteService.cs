using System.Collections.Generic;
using SearchService.Utils;

namespace SearchService.Services
{
    public class TrieAutocompleteService
    {
        private readonly Trie _trie = new();

        public void Insert(string word)
        {
            _trie.Insert(word.ToLowerInvariant());
        }

        public List<string> GetDefaultSuggestions()
        {

            return new List<string>
        {
            "laptop",
            "smartphone",
            "headphones",
            "keyboard",
            "monitor"
        };
        }

        public List<string> GetSuggestions(string prefix)
        {
            return _trie.AutoComplete(prefix);
        }

        public List<string> GetAllWords()
        {
            return _trie.GetAllWords();
        }
    }
}
