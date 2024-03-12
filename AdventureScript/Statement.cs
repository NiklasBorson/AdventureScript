namespace AdventureLib
{
    internal abstract class Statement
    {
        public abstract void Invoke(GameState game, int[] frame);

        public abstract void WriteStatement(GameState game, CodeWriter writer);
    }

    sealed class StatementBlock : Statement
    {
        IList<Statement> m_statements;

        StatementBlock(IList<Statement> statements)
        {
            m_statements = statements;
        }

        public static Statement Create(IList<Statement> statements)
        {
            if (statements.Count == 1)
            {
                return statements[0];
            }
            else
            {
                return new StatementBlock(statements);
            }
        }

        public override void Invoke(GameState game, int[] frame)
        {
            foreach (var statement in m_statements)
            {
                statement.Invoke(game, frame);
            }
        }
        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            foreach (var statement in m_statements)
            {
                statement.WriteStatement(game, writer);
                writer.EndLine();
            }
        }
    }
}
