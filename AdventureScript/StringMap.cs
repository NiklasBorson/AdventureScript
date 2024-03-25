using System;

namespace AdventureScript
{
    class StringMap
    {
        Dictionary<string, int> m_map = new Dictionary<string, int>();
        List<string> m_list = new List<string>();

        public StringMap()
        {
            AddString(string.Empty);
        }

        int AddString(string value)
        {
            int index = m_list.Count;
            m_list.Add(value);
            m_map.Add(value, index);
            return index;
        }

        public int this[string value]
        {
            get
            {
                int index;
                if (m_map.TryGetValue(value, out index))
                {
                    return index;
                }
                else
                {
                    return AddString(value);
                }
            }
        }

        public string this[int index] => m_list[index];
    }
}
