namespace AdventureScript
{
    abstract class BranchStatement : Statement
    {
        public BranchStatement()
        {
            BlockEnd = new BlockEndStatement();
        }

        public BlockEndStatement BlockEnd { get; }
    }
}
