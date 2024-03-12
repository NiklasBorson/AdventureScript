namespace AdventureLib
{
    class IfStatement : Statement
    {
        record struct Block(Expr Condition, Statement Body);
        List<Block> m_blocks = new List<Block>();
        Statement? m_elseBlock = null;

        public IfStatement(Parser parser, Expr expr, Statement body)
        {
            AddBlock(parser, expr, body);
        }

        public void AddBlock(Parser parser, Expr expr, Statement body)
        {
            if (expr.Type != Types.Bool)
            {
                parser.Fail("Expected Boolean expression.");
            }
            m_blocks.Add(new Block(expr, body));
        }

        public void SetElseBlock(Statement body)
        {
            m_elseBlock = body;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            foreach (var block in m_blocks)
            {
                if (block.Condition.Evaluate(game, frame) != 0)
                {
                    block.Body.Invoke(game, frame);
                    return;
                }
            }

            if (m_elseBlock != null)
            {
                m_elseBlock.Invoke(game, frame);
            }
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            for (int i = 0; i < m_blocks.Count; i++)
            {
                writer.Write(i == 0 ? "if" : "else");
                writer.Write(" (");
                m_blocks[i].Condition.WriteExpr(game, writer);
                writer.Write(")");
                writer.BeginBlock();
                m_blocks[i].Body.WriteStatement(game, writer);
                writer.EndBlock();
            }

            if (m_elseBlock != null)
            {
                writer.Write("else");
                writer.BeginBlock();
                m_elseBlock.WriteStatement(game, writer);
                writer.EndBlock();
            }
        }
    }
}
