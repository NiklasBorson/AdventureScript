﻿namespace AdventureScript
{
    public sealed class EnumTypeDef : TypeDef
    {
        public EnumTypeDef(
            SourcePos sourcePos,
            string[] docComments,
            string name, 
            IList<string> valueNames
            ) : base(name, valueNames)
        {
            if (valueNames.Count == 0)
            {
                throw new ArgumentException("An enum type must have at least one value name.");
            }

            this.SourcePos = sourcePos;
            this.DocComments = docComments;
        }

        public SourcePos SourcePos { get; }
        public string[] DocComments { get; }

        public override bool IsUserType => true;

        public override void SaveDefinition(TextWriter writer)
        {
            writer.Write($"enum {Name}({this.ValueNames[0]}");
            for (int i = 1; i < this.ValueNames.Count; i++)
            {
                writer.Write($",{this.ValueNames[i]}");
            }
            writer.Write(");");
        }
        public override void WriteValue(GameState game, int value, TextWriter writer)
        {
            writer.Write($"{this.Name}.{this.ValueNames[value]}");
        }
        public override string ValueToString(GameState game, int value)
        {
            return this.ValueNames[value];
        }
    }
}
