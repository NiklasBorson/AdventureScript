namespace AdventureScript
{
    abstract class Statement
    {
        public int NextStatementIndex { get; set; }

        // Returns the index of the next statement to execute, or an index
        // past the end of the last statement.
        public abstract int Invoke(GameState game, int[] frame);

        public abstract void WriteStatement(GameState game, CodeWriter writer);
    }
}
