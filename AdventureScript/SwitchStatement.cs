using System.Diagnostics;

namespace AdventureScript
{
    sealed class CaseStatement : DummyStatement
    {
        TypeDef m_type;
        int m_value;

        public CaseStatement(TypeDef type, int value)
        {
            m_type = type;
            m_value = value;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("case ");
            m_type.WriteValue(game, m_value, writer.TextWriter);
            writer.BeginBlock();
        }
    }

    sealed class DefaultCaseStatement : DummyStatement
    {
        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("default");
            writer.BeginBlock();
        }
    }


    sealed class SwitchStatement : BranchStatement
    {
        Expr m_expr;
        Dictionary<int, CaseStatement> m_cases = new Dictionary<int, CaseStatement>();
        DefaultCaseStatement? m_defaultCase = null;

        public SwitchStatement(Expr expr)
        {
            m_expr = expr;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            int value = m_expr.Evaluate(game, frame);

            CaseStatement? branch;
            if (m_cases.TryGetValue(value, out branch))
            {
                return branch.NextStatementIndex;
            }
            else if (m_defaultCase != null)
            {
                return m_defaultCase.NextStatementIndex;
            }
            else
            {
                return BlockEnd.NextStatementIndex;
            }
        }

        public int CaseCount => m_cases.Count;

        public CaseStatement? TryCreateCase(int value)
        {
            var statement = new CaseStatement(m_expr.Type, value);
            return m_cases.TryAdd(value, statement) ? statement : null;
        }

        public DefaultCaseStatement CreateDefaultCase()
        {
            Debug.Assert(m_defaultCase == null);
            m_defaultCase = new DefaultCaseStatement();
            return m_defaultCase;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("switch (");
            m_expr.WriteExpr(game, writer);
            writer.Write(")");
            writer.BeginBlock();
        }
    }
}
