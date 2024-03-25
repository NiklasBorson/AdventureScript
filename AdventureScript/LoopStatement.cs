namespace AdventureScript
{
    abstract class LoopStatement : Statement
    {
        public LoopStatement()
        {
            EndStatement = new LoopEndStatement(this);
        }

        public LoopStatement? Parent { get; set; }

        public LoopEndStatement EndStatement { get; }

        // Invoke is called only on the first iteration. Subsequent
        // iterations call InvokeNext.
        public abstract int InvokeNext(GameState game, int[] frame);
    }

    sealed class LoopEndStatement : Statement
    {
        public LoopEndStatement(LoopStatement loop)
        {
            this.Loop = loop;
        }

        public LoopStatement Loop { get; }

        public override int Invoke(GameState game, int[] frame)
        {
            return Loop.InvokeNext(game, frame);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.EndBlock();
        }
    }

    sealed class ContinueStatement : Statement
    {
        LoopStatement Loop { get; }

        public ContinueStatement(LoopStatement loop)
        {
            Loop = loop;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            return Loop.InvokeNext(game, frame);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("continue;");
            writer.EndLine();
        }
    }

    sealed class BreakStatement : Statement
    {
        LoopStatement Loop { get; }

        public BreakStatement(LoopStatement loop)
        {
            Loop = loop;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            return Loop.EndStatement.NextStatementIndex;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("break;");
            writer.EndLine();
        }
    }
}