using System.Diagnostics;

namespace AdventureScript
{
    internal abstract class Expr
    {
        public abstract TypeDef Type { get; }

        public abstract int Evaluate(GameState game, int[] frame);

        public virtual void SetValue(GameState game, int[] frame, int value)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanSetValue => false;
        public abstract bool HasSideEffects { get; }
        public abstract bool IsConstant { get; }

        public abstract Precedence Precedence { get; }

        public abstract void WriteExpr(GameState game, CodeWriter writer);

        public static void WriteSubExpr(
            GameState game, 
            Expr expr, 
            Precedence outerPrecedence, 
            CodeWriter writer
            )
        {
            if (expr.Precedence < outerPrecedence)
            {
                writer.Write("(");
                expr.WriteExpr(game, writer);
                writer.Write(")");
            }
            else
            {
                expr.WriteExpr(game, writer);
            }
        }

        protected void WriteSubExpr(GameState game, Expr expr, CodeWriter writer)
        {
            WriteSubExpr(game, expr, this.Precedence, writer);
        }

        public int EvaluateConst(GameState game)
        {
            Debug.Assert(this.IsConstant);
            return Evaluate(game, m_emptyFrame);
        }

        static readonly int[] m_emptyFrame = new int[0];
    }
}
