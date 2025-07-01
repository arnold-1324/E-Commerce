using System.Collections.Generic;


namespace SearchService.Utils
{

    public class TrieNode
    {
        public Dictionary<char, TrieNode> Children = new();
        public bool IsEndOfWord = false;
    }

    public class Trie
    {
        private readonly TrieNode _root = new(); // Also fixes CS8618

        public void Insert(string word)
        {
            var current = _root;

            foreach (var c in word)
            {
                if (!current.Children.ContainsKey(c))
                    current.Children[c] = new TrieNode();
                current = current.Children[c];
            }
            current.IsEndOfWord = true;
        }

        public bool Search(string word)
        {
            var current = _root;
            foreach (var c in word)
            {
                if (!current.Children.ContainsKey(c))
                    return false;
                current = current.Children[c];
            }
            return current.IsEndOfWord;
        }

        public List<string> AutoComplete(string prefix)
        {
            var results = new List<string>();
            var current = _root;

            foreach (char c in prefix)
            {
                if (!current.Children.ContainsKey(c))
                    return results;
                current = current.Children[c];
            }

            DFS(current, prefix, results);
            return results;
        }

        private void DFS(TrieNode node, string currentWord, List<string> results)
        {
            if (node.IsEndOfWord)
                results.Add(currentWord);

            foreach (var kvp in node.Children)
            {
                DFS(kvp.Value, currentWord + kvp.Key, results);
            }
        }

        public List<string> GetAllWords()
        {
            var results = new List<string>();
            DFS(_root, "", results);
            return results;
        }
    }


}