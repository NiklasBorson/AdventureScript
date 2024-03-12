namespace AdventureLib
{
    internal class WordMapEntry
    {
        static readonly string[] m_emptyList = new string[0];
        string[] m_adjectives = m_emptyList;

        public WordMapEntry(int itemId)
        {
            this.ItemId = itemId;
            this.Noun = string.Empty;
        }

        public void AddAdjective(string word)
        {
            for (int i = 0; i < m_adjectives.Length; i++)
            {
                if (m_adjectives[i] == word)
                    return;
            }

            var newArray = new string[m_adjectives.Length + 1];
            Array.Copy(m_adjectives, newArray, m_adjectives.Length);
            newArray[m_adjectives.Length] = word;
            m_adjectives = newArray;
        }

        public int ItemId { get; }
        public string Noun { get; set; }
        public IReadOnlyList<string> Adjectives => m_adjectives;
        public WordMapEntry? NextEntry { get; set; }
    }

    class WordMap
    {
        Dictionary<string, WordMapEntry> m_nounMap = new Dictionary<string, WordMapEntry>();
        WordMapEntry?[] m_itemEntries;

        public WordMap(int itemCount)
        {
            m_itemEntries = new WordMapEntry[itemCount];
        }

        public void AddNoun(string value, int itemId)
        {
            if (value == string.Empty)
                return;

            // Make sure the item ID is in range.
            if (itemId > 0 && itemId < m_itemEntries.Length)
            {
                var entry = m_itemEntries[itemId];
                if (entry == null)
                {
                    // Lazily create a new MapEntry for this item.
                    entry = new WordMapEntry(itemId);
                    m_itemEntries[itemId] = entry;
                }
                else if (entry.Noun != string.Empty)
                {
                    // Can only add one noun per item.
                    return;
                }

                // Set the noun.
                entry.Noun = value.ToLowerInvariant();

                // We're going to store this entry in the map. If there is
                // already an existing entry for this noun, let this entry
                // point to the existing entry, so we're inserting this entry
                // at the head of a linked list of entries for the noun.
                WordMapEntry? existingEntry;
                if (m_nounMap.TryGetValue(entry.Noun, out existingEntry))
                {
                    entry.NextEntry = existingEntry;
                }
                m_nounMap[entry.Noun] = entry;
            }
        }

        public void AddAdjective(string value, int itemId)
        {
            // Make sure the item ID is in range.
            if (itemId > 0 && itemId < m_itemEntries.Length)
            {
                var entry = m_itemEntries[itemId];
                if (entry == null)
                {
                    // Lazily create a new MapEntry for this item.
                    entry = new WordMapEntry(itemId);
                    m_itemEntries[itemId] = entry;
                }

                entry.AddAdjective(value.ToLowerInvariant());
            }
        }

        static bool MatchAdjectives(
            Span<string> inputAdjectives,
            IReadOnlyList<string> itemAdjectives
            )
        {
            foreach (string word in inputAdjectives)
            {
                if (!itemAdjectives.Contains(word))
                    return false;
            }
            return true;
        }

        public IReadOnlyList<WordMapEntry> GetMatches(
            string inputNoun,
            Span<string> inputAdjectives
            )
        {
            var result = new List<WordMapEntry>();
            WordMapEntry? firstEntry;
            if (m_nounMap.TryGetValue(inputNoun, out firstEntry))
            {
                for (WordMapEntry? entry = firstEntry; entry != null; entry = entry.NextEntry)
                {
                    if (MatchAdjectives(inputAdjectives, entry.Adjectives))
                    {
                        result.Add(entry);
                    }
                }
            }
            return result;
        }

        public static IReadOnlyList<WordMapEntry> FilterMatches(
            IReadOnlyList<WordMapEntry> inputList,
            Span<string> inputAdjectives
            )
        {
            var list = new List<WordMapEntry>();
            foreach (var item in inputList)
            {
                if (MatchAdjectives(inputAdjectives, item.Adjectives))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public IList<WordMapEntry> GetAllWords()
        {
            var itemWords = new List<WordMapEntry>();
            foreach (var firstEntry in m_nounMap.Values)
            {
                for (WordMapEntry? entry = firstEntry; entry != null; entry = entry.NextEntry)
                {
                    itemWords.Add(entry);
                }
            }
            return itemWords;
        }
    }
}
