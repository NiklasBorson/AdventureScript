using System.Collections;

namespace AdventureLib
{
    class PropertyMap : IEnumerable<PropertyDef>
    {
        Dictionary<string, PropertyDef> m_map = new Dictionary<string, PropertyDef>();

        public bool TryAdd(string name, TypeDef type)
        {
            var def = new PropertyDef(name, type);
            return m_map.TryAdd(name, def);
        }

        public PropertyDef? TryGet(string name)
        {
            PropertyDef? def;
            return m_map.TryGetValue(name, out def) ? def : null;
        }

        public bool Exists(string name) => m_map.ContainsKey(name);

        public void Save(TextWriter writer)
        {
            foreach (var def in m_map.Values)
            {
                writer.WriteLine($"property {def.Name}: {def.Type.Name};");
            }
        }

        IEnumerator<PropertyDef> IEnumerable<PropertyDef>.GetEnumerator()
        {
            return m_map.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_map.Values.GetEnumerator();
        }
    };
}
