namespace AdventureScript
{
    sealed class IfStatement : BranchStatement
    {
        Expr m_expr;
        string m_keyword;

        public IfStatement(Expr expr, string keyword)
        {
            m_expr = expr;
            m_keyword = keyword;
            ElseBranch = this.BlockEnd;
        }

        public Statement ElseBranch { get; set; }

        public override int Invoke(GameState game, int[] frame)
        {
            return m_expr.Evaluate(game, frame) != 0 ?
                NextStatementIndex :
                ElseBranch.Invoke(game, frame);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write(m_keyword);
            writer.Write(" (");
            m_expr.WriteExpr(game, writer);
            writer.Write(")");
            writer.BeginBlock();
        }
    }

    sealed class ElseStatement : DummyStatement
    {
        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("else");
            writer.BeginBlock();
        }
    }
}
