namespace AdventureLib
{
    class PropertyDef
    {
        // Array of values indexed by object ID.
        int[] m_values = new int[8];

        public PropertyDef(string name, TypeDef typeDef)
        {
            this.Name = name;
            this.Type = typeDef;
        }

        public string Name { get; }

        public TypeDef Type { get; }

        public void Clear()
        {
            for (int i = 0; i < m_values.Length; i++)
            {
                m_values[i] = 0;
            }
        }

        public int this[int index]
        {
            get
            {
                return index < m_values.Length ? m_values[index] : 0;
            }

            set
            {
                // Do nothing if it's the "null" item (objectId == 0).
                if (index > 0)
                {
                    // Lazily reallocate the array of values if needed.
                    int count = m_values.Length;
                    if (index >= count)
                    {
                        // Double the size as many times as necessary for the
                        // index to be in range.
                        int newCount = count * 2;
                        while (index >= newCount)
                        {
                            newCount += newCount;
                        }

                        // Create the new array, copying the old values.
                        var newValues = new int[newCount];
                        m_values.CopyTo(newValues, 0);
                        m_values = newValues;
                    }

                    // Store the value.
                    m_values[index] = value;
                }
            }
        }
    }
}
