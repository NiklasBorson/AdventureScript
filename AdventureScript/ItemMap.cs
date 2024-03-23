namespace AdventureLib
{
    internal record class Item(string Name, int ID);

    class ItemMap
    {
        Dictionary<string, Item> m_map = new Dictionary<string, Item>();
        List<Item> m_list = new List<Item>();

        public ItemMap()
        {
            // The implicitly defined "null" item is added first and has ID 0.
            AddItem("null");
        }

        public Item AddItem(string name)
        {
            var item = new Item(name, /*id*/ m_list.Count);
            if (!m_map.TryAdd(name, item))
            {
                throw new ArgumentException($"Item \"{name}\" is already defined.");
            }
            m_list.Add(item);
            return item;
        }

        public bool Exists(string name) => m_map.ContainsKey(name);

        public Item? TryGetItem(string name)
        {
            Item? item;
            return m_map.TryGetValue(name, out item) ? item : null;
        }

        public Item this[string name]
        {
            get
            {
                Item? item;
                if (m_map.TryGetValue(name, out item))
                {
                    return item;
                }
                throw new ArgumentException($"Item \"{name}\" is not defined.");
            }
        }

        public Item this[int id] => m_list[id];

        public int Count => m_list.Count;

        public void SaveDefinitions(TextWriter writer)
        {
            for (int id = 1; id < m_list.Count; id++)
            {
                string name = m_list[id].Name;
                if (!Lexer.IsName(name))
                {
                    name = Lexer.Stringize(name);
                }

                writer.WriteLine($"item {name};");
            }
        }

        public void SaveProperties(GameState game, CodeWriter writer)
        {
            for (int id = 1; id < m_list.Count; id++)
            {
                writer.BeginBlock();

                writer.Write("var $_ = ");

                string name = m_list[id].Name;
                if (Lexer.IsName(name))
                {
                    writer.Write(name);
                }
                else
                {
                    writer.Write("GetItem(");
                    writer.Write(Lexer.Stringize(name));
                    writer.Write(")");
                }

                writer.Write(";");
                writer.EndLine();

                foreach (var prop in game.Properties)
                {
                    int value = prop[id];
                    if (value != 0)
                    {
                        writer.Write($"$_.{prop.Name} = ");
                        prop.Type.WriteValue(game, value, writer.TextWriter);
                        writer.Write(";");
                        writer.EndLine();
                    }
                }

                writer.EndBlock();
            }
            // TODO
        }
    }
}
