﻿namespace AdventureScript
{
    public class DelegateTypeDef : TypeDef
    {
        public DelegateTypeDef(
            SourcePos sourcePos,
            string[] docComments,
            string name, 
            IList<ParamDef> paramDefs, 
            TypeDef returnType
            ) : base(name)
        {
            this.SourcePos = sourcePos;
            this.DocComments = docComments;
            this.ParamList = paramDefs;
            this.ReturnType = returnType;
        }
        public SourcePos SourcePos { get; }
        public string[] DocComments { get; }

        public IList<ParamDef> ParamList { get; }
        public TypeDef ReturnType { get; }

        public bool IsMatch(IList<ParamDef> paramDefs, TypeDef returnType)
        {
            int paramCount = paramDefs.Count;
            if (paramCount != this.ParamList.Count)
                return false;

            for (int i = 0; i < paramCount; i++)
            {
                if (paramDefs[i].Type != this.ParamList[i].Type)
                    return false;
            }

            return returnType == this.ReturnType;
        }

        public override bool IsUserType => true;

        public override void SaveDefinition(TextWriter writer)
        {
            writer.Write($"delegate {Name}");
            FunctionDef.WriteParamList(this.ParamList, writer);
            FunctionDef.WriteReturnType(this.ReturnType, writer);
            writer.Write(';');
        }
        public override void WriteValue(GameState game, int value, TextWriter writer)
        {
            writer.Write(game.Functions[value].Name);
        }
        public override string ValueToString(GameState game, int value)
        {
            return this.ValueNames[value];
        }
    }
}
