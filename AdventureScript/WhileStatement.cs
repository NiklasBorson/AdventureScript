using AdventureLib;

namespace AdventureScript
{
    sealed class WhileStatement : Statement
    {
        Expr m_condition;
        Statement m_body;

        public WhileStatement(Parser parser, Expr expr, Statement body)
        {
            if (expr.Type != Types.Bool)
            {
                parser.Fail("Expected Boolean expression.");
            }
            m_condition = expr;
            m_body = body;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            while (m_condition.Evaluate(game, frame) != 0)
            {
                m_body.Invoke(game, frame);
            }
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("while (");
            m_condition.WriteExpr(game, writer);
            writer.Write(")");
            writer.BeginBlock();
            m_body.WriteStatement(game, writer);
            writer.EndBlock();
        }
    }
}
