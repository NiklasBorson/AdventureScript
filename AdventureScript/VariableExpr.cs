namespace AdventureScript
{
    internal abstract class VariableExprBase : Expr
    {
        TypeDef m_type;

        public VariableExprBase(string name, TypeDef type)
        {
            this.Name = name;
            m_type = type;
        }

        public string Name { get; }

        public override sealed TypeDef Type => m_type;

        public void SetType(TypeDef newType)
        {
            m_type = newType;
        }
        public override sealed bool HasSideEffects => false;

        public override bool CanSetValue => true;

        public override bool IsConstant => false;

        public override sealed Precedence Precedence => Precedence.Atomic;

        public override sealed void WriteExpr(GameState game, CodeWriter writer)
        {
            writer.Write(this.Name);
        }
    }

    sealed class VariableExpr : VariableExprBase
    {
        public VariableExpr(string name, TypeDef type, int frameIndex) : 
            base(name, type)
        {
            this.FrameIndex = frameIndex;
        }

        public int FrameIndex { get; }

        public override int Evaluate(GameState game, int[] frame)
        {
            return frame[FrameIndex];
        }

        public override void SetValue(GameState game, int[] frame, int value)
        {
            frame[FrameIndex] = value;
        }
    }
}
