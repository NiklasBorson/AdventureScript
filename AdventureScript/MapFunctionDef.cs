using System.Diagnostics;
using System.Reflection.Emit;

namespace AdventureScript
{
    class MapFunctionDef : FunctionDef
    {
        TypeDef m_fromType;
        TypeDef m_toType;
        int[] m_valueMap;

        public MapFunctionDef(string name, TypeDef fromType, TypeDef toType, int[] valueMap) : base(
            name,
            new ParamDef[] { new ParamDef("$fromValue", fromType) },
            toType
            )
        {
            Debug.Assert(fromType.IsEnumType);
            Debug.Assert(valueMap.Length == fromType.ValueNames.Count);

            m_fromType = fromType;
            m_toType = toType;
            m_valueMap = valueMap;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            int fromValue = frame[1];
            return m_valueMap[fromValue];
        }

        public override int FrameSize => ParamList.Count + 1;

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
            // "map" <name> <fromType> "->" <toType>
            writer.Write("map ");
            writer.Write(this.Name);
            writer.Write(" ");
            writer.Write(m_fromType.Name);
            writer.Write(" -> ");
            writer.Write(m_toType.Name);

            writer.BeginBlock();

            for (int i = 0; i < m_valueMap.Length; i++)
            {
                // <fromValue> "->"
                writer.Write(m_fromType.ValueNames[i]);
                writer.Write(" -> ");

                // Write the "to" value.
                int value = m_valueMap[i];
                if (m_toType.IsEnumType)
                {
                    writer.Write(m_toType.ValueNames[value]);
                }
                else
                {
                    m_toType.WriteValue(game, value, writer.TextWriter);
                }

                // Write a comma after all but the last entry.
                if (i + 1 < m_valueMap.Length)
                {
                    writer.Write(",");
                }
                writer.EndLine();
            }

            writer.EndBlock();
        }
    }
}
