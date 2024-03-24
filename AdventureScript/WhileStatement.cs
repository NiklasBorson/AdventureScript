namespace AdventureScript
{
    sealed class WhileStatement : LoopStatement
    {
        Expr m_condition;

        public WhileStatement(Parser parser, Expr expr)
        {
            if (expr.Type != Types.Bool)
            {
                parser.Fail("Expected Boolean expression.");
            }
            m_condition = expr;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            return InvokeNext(game, frame);
        }

        public override int InvokeNext(GameState game, int[] frame)
        {
            return m_condition.Evaluate(game, frame) != 0 ?
                NextStatementIndex :
                EndStatement.NextStatementIndex;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("while (");
            m_condition.WriteExpr(game, writer);
            writer.Write(")");
            writer.BeginBlock();
        }
    }
}
