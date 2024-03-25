using System.Diagnostics;

namespace AdventureScript
{
    sealed class ForEachEnumStatement : LoopStatement
    {
        VariableExpr m_loopVar;
        int m_frameIndex;
        TypeDef m_type;
        int m_valueCount;

        public ForEachEnumStatement(VariableExpr loopVar, TypeDef type)
        {
            m_loopVar = loopVar;
            m_frameIndex = loopVar.FrameIndex;
            m_type = type;
            m_valueCount = type.ValueNames.Count;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            // First iteration starts with value zero.
            frame[m_frameIndex] = 0;
            return NextStatementIndex;
        }

        public override int InvokeNext(GameState game, int[] frame)
        {
            // Subsequent iterations increment the value until we fall
            // out of the loop.
            int value = frame[m_frameIndex] + 1;
            if (value < m_valueCount)
            {
                frame[m_frameIndex] = value;
                return NextStatementIndex;
            }
            else
            {
                return this.EndStatement.NextStatementIndex;
            }
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write($"foreach (var {m_loopVar.Name}:{m_type.Name})");
            writer.BeginBlock();
        }
    }
}
