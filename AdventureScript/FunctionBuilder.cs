using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace AdventureScript
{
    class FunctionBuilder : VariableFrame
    {
        List<Statement> m_statements = new List<Statement>();
        LoopStatement? m_currentLoop = null;

        public FunctionBuilder(Parser parser) : base(parser)
        {
        }

        public void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
            statement.NextStatementIndex = m_statements.Count;
        }

        public void BeginLoop(LoopStatement loop)
        {
            loop.Parent = m_currentLoop;
            m_currentLoop = loop;
            AddStatement(loop);
        }

        public void EndLoop(LoopStatement loop)
        {
            Debug.Assert(loop == m_currentLoop);

            AddStatement(loop.EndStatement);
            m_currentLoop = loop.Parent;
        }

        public LoopStatement? CurrentLoop => m_currentLoop;

        public FunctionBody CreateFunctionBody()
        {
            ElideDummyStatements();
            return new FunctionBody(FrameSize, m_statements);
        }

        void ElideDummyStatements()
        {
            // Optimization so we skip over dummy statements instead of executing them.
            int count = m_statements.Count;
            foreach (var statement in m_statements)
            {
                // If a statement's next statement is a dummy statement, point to the dummy
                // statement's next statement instead. If that is also a dummy statement,
                // point its next statement, and so on.
                int next = statement.NextStatementIndex;
                if (next < count && m_statements[next] is DummyStatement)
                {
                    do
                    {
                        next = m_statements[next].NextStatementIndex;
                    }
                    while (next < count && m_statements[next] is DummyStatement);
                    statement.NextStatementIndex = next;
                }
            }
        }
    }

    class FunctionBody
    {
        Statement[] m_statements;

        public FunctionBody(int frameSize, IList<Statement> statements)
        {
            this.FrameSize = frameSize;
            m_statements = statements.ToArray();
        }

        public int FrameSize { get; }

        public void Invoke(GameState game, int[] frame)
        {
            int i = 0;
            while (i < m_statements.Length)
            {
                i = m_statements[i].Invoke(game, frame);
            }
        }

        public void Invoke(GameState game)
        {
            Invoke(game, new int[FrameSize]);
        }

        public void Write(GameState game, CodeWriter writer)
        {
            writer.BeginBlock();

            foreach (var statement in m_statements)
            {
                statement.WriteStatement(game, writer);
            }

            writer.EndBlock();
        }
    }
}
