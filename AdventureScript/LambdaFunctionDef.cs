using System.Reflection.Emit;

namespace AdventureScript
{
    class LambdaFunctionDef : FunctionDef
    {
        int m_frameSize;
        Expr m_expr;

        public LambdaFunctionDef(string name, IList<ParamDef> paramList, int frameSize, Expr expr) :
            base(name, paramList, expr.Type)
        {
            m_frameSize = frameSize;
            m_expr = expr;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            return m_expr.Evaluate(game, frame);
        }

        public override int FrameSize => m_frameSize;

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
            // Write "function" <name> "(" <ParamList> ")"
            WriteDeclaration(writer);

            // Write the lambda expression.
            writer.Write(" => ");
            m_expr.WriteExpr(game, writer);
            writer.Write(";");

            writer.EndLine();
        }
    }
}
