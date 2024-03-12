namespace AdventureLib
{
    sealed class UserFunctionDef : FunctionDef
    {
        int m_frameSize;
        Statement m_code;

        public UserFunctionDef(string name, IList<ParamDef> paramList, TypeDef returnType, int frameSize, Statement code) :
            base(name, paramList, returnType)
        {
            m_frameSize = frameSize;
            m_code = code;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            m_code.Invoke(game, frame);
            return frame[0];
        }

        public override int FrameSize => m_frameSize;

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
            // Write "function" <name> "(" <ParamList> ")"
            WriteDeclaration(writer);

            // Write ( ":" <typeName )?
            WriteReturnType(this.ReturnType, writer.TextWriter);

            // Write the function body.
            writer.BeginBlock();
            m_code.WriteStatement(game, writer);
            writer.EndBlock();
        }
    }
}
