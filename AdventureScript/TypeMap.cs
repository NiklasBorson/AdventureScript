using System.Collections;
using System.Xml.Linq;

namespace AdventureScript
{
    class TypeMap : IEnumerable<TypeDef>
    {
        Dictionary<string, TypeDef> m_map = new Dictionary<string, TypeDef>();
        List<DelegateTypeDef> m_delegateTypes = new List<DelegateTypeDef>();

        public TypeMap()
        {
            foreach (var type in Types.StandardRealTypes)
            {
                m_map.Add(type.Name, type);
            }
        }

        public EnumTypeDef AddEnumType(string name, IList<string> valueNames)
        {
            var newType = new EnumTypeDef(name, valueNames);
            Add(newType);
            return newType;
        }

        public DelegateTypeDef? FindDelegateType(IList<ParamDef> paramDefs, TypeDef returnType)
        {
            foreach (var existingType in m_delegateTypes)
            {
                if (existingType.IsMatch(paramDefs, returnType))
                {
                    return existingType;
                }
            }
            return null;
        }

        public DelegateTypeDef AddDelegateType(string name, IList<ParamDef> paramDefs, TypeDef returnType)
        {
            // Look for an existing type that matches the parameters.
            var existingType = FindDelegateType(paramDefs, returnType);
            if (existingType != null)
            {
                // Add the name as an alias for the existing type.
                m_map.Add(name, existingType);
                return existingType;
            }
            else
            {
                // This is a distinct delegate type.
                var newType = new DelegateTypeDef(name, paramDefs, returnType);

                // Add the new type to the map and the delegate list.
                Add(newType);
                m_delegateTypes.Add(newType);
                return newType;
            }
        }

        void Add(TypeDef def)
        {
            m_map.Add(def.Name, def);
        }

        public bool Exists(string name) => m_map.ContainsKey(name);

        public TypeDef? TryGet(string name)
        {
            TypeDef? def;
            return m_map.TryGetValue(name, out def) ? def : null;
        }

        public TypeDef this[string name]
        {
            get
            {
                TypeDef? def;
                if (m_map.TryGetValue(name, out def))
                {
                    return def;
                }
                throw new ArgumentException($"Type {name} is not defined.");
            }
        }

        public void Save(TextWriter writer)
        {
            foreach (var type in m_map.Values)
            {
                if (type.IsUserType)
                {
                    type.SaveDefinition(writer);
                    writer.WriteLine();
                }
            }
        }

        public IEnumerator<TypeDef> GetEnumerator()
        {
            return m_map.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_map.Values.GetEnumerator();
        }
    }
}
