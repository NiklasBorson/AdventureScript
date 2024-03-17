using System.Collections;

namespace AdventureLib
{
    class FunctionMap : IEnumerable<FunctionDef>
    {
        Dictionary<string, FunctionDef> m_map = new Dictionary<string, FunctionDef>();
        List<FunctionDef> m_list = new List<FunctionDef>();

        public FunctionMap()
        {
            // ID zero is the null function def.
            m_list.Add(new NullFunctionDef());

            // Add intrinsic functions.
            IntrinsicFunctions.Add(this);
        }

        public void Add(FunctionDef def)
        {
            def.ID = m_list.Count;
            m_map.Add(def.Name, def);
            m_list.Add(def);
        }

        public bool Exists(string name) => m_map.ContainsKey(name);

        public FunctionDef? TryGetFunction(string name)
        {
            FunctionDef? def;
            return m_map.TryGetValue(name, out def) ? def : null;
        }

        public void SaveDefinitions(GameState game, CodeWriter writer)
        {
            foreach (var func in m_list)
            {
                func.SaveDefinition(game, writer);
            }
        }

        public int Count => m_list.Count;

        public FunctionDef this[int index] => m_list[index];

        IEnumerator<FunctionDef> IEnumerable<FunctionDef>.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
    }
}
