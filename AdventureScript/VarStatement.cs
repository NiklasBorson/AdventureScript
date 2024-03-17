using AdventureLib;
using System.Diagnostics;

namespace AdventureScript
{
    sealed class VarStatement : Statement
    {
        VariableExpr m_var;
        Expr? m_rightExpr;

        public VarStatement(VariableExpr newVar, Expr? rightExpr)
        {
            m_var = newVar;
            m_rightExpr = rightExpr;

            // The caller should have already verified that the types are compatible.
            Debug.Assert(m_rightExpr == null ||
                AssignStatement.CanAssignTypes(m_var.Type, m_rightExpr.Type));
        }

        public override void Invoke(GameState game, int[] frame)
        {
            int value = m_rightExpr?.Evaluate(game, frame) ?? 0;
            m_var.SetValue(game, frame, value);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write($"var {m_var.Name}");

            if (m_rightExpr == null || m_rightExpr.Type == Types.Null)
            {
                writer.Write($" : {m_var.Type.Name}");
            }
            else
            {
                writer.Write(" = ");
                m_rightExpr.WriteExpr(game, writer);
            }
            writer.Write(";");
        }
    }
}
