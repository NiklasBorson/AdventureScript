using System.Reflection.Emit;
using System.Text;

namespace AdventureScript
{
    public record struct ParamDef(string Name, TypeDef Type);

    public abstract class FunctionDef
    {
        public int ID { get; set; }
        public string Name { get; }
        public IList<ParamDef> ParamList { get; }
        public TypeDef ReturnType { get; }

        public FunctionDef(string name, IList<ParamDef> paramList, TypeDef returnType)
        {
            this.Name = name;
            this.ParamList = paramList;
            this.ReturnType = returnType;
        }

        public abstract int Invoke(GameState game, int[] frame);

        public abstract int FrameSize { get; }

        public void GetDeclaration(StringBuilder b)
        {
            b.Append($"function {Name}(");
            for (int i = 0; i < ParamList.Count; i++)
            {
                var paramDef = ParamList[i];
                if (i != 0)
                {
                    b.Append(", ");
                }
                b.Append($"{paramDef.Name}:{paramDef.Type.Name}");
            }
            if (ReturnType != Types.Void)
            {
                b.Append($") : {ReturnType.Name};");
            }
            else
            {
                b.Append(");");
            }
        }

        public abstract void SaveDefinition(GameState game, CodeWriter writer);

        // Write "function" <name> "(" <ParamList> ")"
        protected void WriteDeclaration(CodeWriter writer)
        {
            writer.Write("function ");
            writer.Write(this.Name);
            WriteParamList(this.ParamList, writer.TextWriter);
        }

        // Write "(" <ParamList> ")"
        public static void WriteParamList(IList<ParamDef> paramList, TextWriter writer)
        {
            // Write the parameter list.
            writer.Write('(');
            if (paramList.Count != 0)
            {
                WriteParam(paramList[0], writer);
                for (int i = 1; i < paramList.Count; i++)
                {
                    writer.Write(", ");
                    WriteParam(paramList[i], writer);
                }
            }
            writer.Write(')');
        }

        // Write ( ":" <typeName )?
        public static void WriteReturnType(TypeDef returnType, TextWriter writer)
        {
            if (returnType != Types.Void)
            {
                writer.Write(" : ");
                writer.Write(returnType.Name);
            }
        }

        static void WriteParam(ParamDef param, TextWriter writer)
        {
            writer.Write(param.Name);
            writer.Write(':');
            writer.Write(param.Type.Name);
        }
    }
}
