using System.Diagnostics;

namespace AdventureScript
{
    static class Types
    {
        // A "real" types is added to to the TypeMap and can be specified as the
        // type of a property, variable, or parameter.
        public static readonly TypeDef Item = new ItemType();
        public static readonly TypeDef String = new StringType();
        public static readonly TypeDef Int = new IntType();
        public static readonly TypeDef Bool = new BoolType();

        public static readonly TypeDef[] StandardRealTypes = new TypeDef[]
        {
            Item, String, Int, Bool
        };

        // A "fake" type is never specified explicitly but is used as the type of
        // certain expressions.
        //  - Void is the return type of a function that does not return a value.
        //  - Null is the type of the null keyword.
        public static readonly TypeDef Void = new VoidType();
        public static readonly TypeDef Null = new NullType();

        sealed class VoidType : TypeDef
        {
            public VoidType() : base("void")
            {
            }

            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                // There is no such thing as a void value.
            }

            public override string ValueToString(GameState game, int value)
            {
                // There is no such thing as a void value.
                return string.Empty;
            }
        }

        sealed class NullType : TypeDef
        {
            public NullType() : base("null")
            {
            }

            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                Debug.Assert(value == 0);
                writer.Write("null");
            }

            public override string ValueToString(GameState game, int value)
            {
                Debug.Assert(value == 0);
                return "null";
            }
        }

        sealed class ItemType : TypeDef
        {
            public ItemType() : base("Item")
            {
            }
            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                string name = game.Items[value].Name;
                if (Lexer.IsName(name))
                {
                    writer.Write(name);
                }
                else
                {
                    name = StringHelpers.ToStringLiteral(name);
                    writer.Write($"GetItem({name})");
                }
            }

            public override string ValueToString(GameState game, int value)
            {
                return game.Items[value].Name;
            }
        }
        sealed class StringType : TypeDef
        {
            public StringType() : base("String")
            {
            }
            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                writer.Write(StringHelpers.ToStringLiteral(game.Strings[value]));
            }
            public override string ValueToString(GameState game, int value)
            {
                return game.Strings[value];
            }
        }
        sealed class IntType : TypeDef
        {
            public IntType() : base("Int")
            {
            }
            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                writer.Write(value);
            }
            public override string ValueToString(GameState game, int value)
            {
                return value.ToString();
            }
        }
        sealed class BoolType : TypeDef
        {
            public BoolType() : base("Bool")
            {
            }
            public override bool IsUserType => false;

            public override void WriteValue(GameState game, int value, TextWriter writer)
            {
                writer.Write(value != 0 ? "true" : "false");
            }
            public override string ValueToString(GameState game, int value)
            {
                return value != 0 ? "true" : "false";
            }
        }
    }
}
