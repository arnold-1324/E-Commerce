using System.Collections.Generic;


namespace SearchService.Utils
{
    public class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public List<string> ProductIds { get; } = new();
        public bool IsEndOfWord { get; set; }
    }

    public class Trie
    {
        private readonly TrieNode _root = new();

        public void Insert(string word, string productId)
        {
            var node = _root;
            foreach (char ch in word.ToLower())
            {
                if (!node.Children.ContainsKey(ch))
                    node.Children[ch] = new TrieNode();
                node = node.Children[ch];
                node.ProductIds.Add(productId); // Optional: build prefix index
            }
            node.IsEndOfWord = true;
        }

        public List<string> Search(string prefix)
        {
            var node = _root;
            foreach (char ch in prefix.ToLower())
            {
                if (!node.Children.ContainsKey(ch))
                    return new List<string>();
                node = node.Children[ch];
            }

            return node.ProductIds.Distinct().ToList(); // Avoid duplicates
        }
    }
}