namespace AdventureLib
{
    internal abstract class TypeDef
    {
        static IList<string> m_noNames = new string[0];

        protected TypeDef(string name)
        {
            this.Name = name;
            this.ValueNames = m_noNames;
        }

        protected TypeDef(string name, IList<string> valueNames)
        {
            this.Name = name;
            this.ValueNames = valueNames;
        }

        public string Name { get; }

        public IList<string> ValueNames { get; }

        public bool IsEnumType => ValueNames.Count != 0;

        public virtual void SaveDefinition(TextWriter writer)
        {
            // Default implementation for built-in types does nothing
        }

        public abstract void WriteValue(GameState game, int value, CodeWriter writer);

        public abstract string ValueToString(GameState game, int value);
    }
}
