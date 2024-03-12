namespace AdventureLib
{
    class NullFunctionDef : FunctionDef
    {
        public NullFunctionDef() : base("null", new ParamDef[0], Types.Void)
        {
        }

        public override int FrameSize => 1;

        public override int Invoke(GameState game, int[] frame)
        {
            return 0;
        }

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
        }
    }
}
