namespace AdventureScript
{
    sealed class GlobalVariableExpr : VariableExprBase
    {
        public GlobalVariableExpr(string name, TypeDef type) :
            base(name, type)
        {
        }

        public int Value { get; set; }

        public override int Evaluate(GameState game, int[] frame)
        {
            return this.Value;
        }

        public override void SetValue(GameState game, int[] frame, int value)
        {
            this.Value = value;
        }
    }
}
