namespace AdventureScript
{
    sealed class GlobalVariableExpr : VariableExprBase
    {
        bool m_isConst;

        public GlobalVariableExpr(
            SourcePos sourcePos,
            string[] docComments,
            string name, 
            TypeDef type, 
            bool isConst
            ) :
            base(name, type)
        {
            this.SourcePos = sourcePos;
            this.DocComments = docComments;
            m_isConst = isConst;
        }

        public SourcePos SourcePos { get; }
        public string[] DocComments { get; }

        public int Value { get; set; }

        public override bool CanSetValue => !m_isConst;

        public override bool IsConstant => m_isConst;

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
