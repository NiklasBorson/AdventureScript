namespace AdventureScript
{
    sealed class UserFunctionDef : FunctionDef
    {
        FunctionBody m_body;

        public UserFunctionDef(string name, IList<ParamDef> paramList, TypeDef returnType, FunctionBody body) :
            base(name, paramList, returnType)
        {
            m_body = body;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            m_body.Invoke(game, frame);
            return frame[0];
        }

        public override int FrameSize => m_body.FrameSize;

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
            // Write "function" <name> "(" <ParamList> ")"
            WriteDeclaration(writer);

            // Write ( ":" <typeName )?
            WriteReturnType(this.ReturnType, writer.TextWriter);

            // Write the function body.
            m_body.Write(game, writer);
        }
    }
}
