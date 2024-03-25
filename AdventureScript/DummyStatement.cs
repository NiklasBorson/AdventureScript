namespace AdventureScript
{
    // Base class for a statement that does nothing but exists merely
    // for syntactic reasons.
    abstract class DummyStatement : Statement
    {
        public sealed override int Invoke(GameState game, int[] frame)
        {
            return NextStatementIndex;
        }
    }

    sealed class BlockStartStatement : DummyStatement
    {
        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.BeginBlock();
        }
    }

    sealed class BlockEndStatement : DummyStatement
    {
        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.EndBlock();
        }
    }
}
