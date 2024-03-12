using System.Diagnostics;

namespace AdventureLib
{
    internal enum Precedence
    {
        None,
        Ternary,
        AndOr,
        Compare,
        AddSub,
        MulDiv,
        UnaryNegative,
        Member,
        Atomic
    }

    internal record class BinaryOp(
        SymbolId SymbolId,
        string SymbolText,
        Precedence Precedence,
        BinaryExpr.DeriveType DeriveType,
        BinaryExpr.Compute Compute
        );

    static class BinaryOperators
    {
        static readonly BinaryOp[] m_ops = new BinaryOp[]
        {
            new BinaryOp(
                SymbolId.Equals,
                "==",
                Precedence.Compare,
                EqualyCompare_DeriveType,
                Equals_Compute
                ),
            new BinaryOp(
                SymbolId.NotEquals,
                "!=",
                Precedence.Compare,
                EqualyCompare_DeriveType,
                NotEquals_Compute
                ),
            new BinaryOp(
                SymbolId.Less,
                "<",
                Precedence.Compare,
                InequalyCompare_DeriveType,
                LessThan_Compute
                ),
            new BinaryOp(
                SymbolId.LessEquals,
                "<=",
                Precedence.Compare,
                InequalyCompare_DeriveType,
                LessEquals_Compute
                ),
            new BinaryOp(
                SymbolId.Greater,
                ">",
                Precedence.Compare,
                InequalyCompare_DeriveType,
                GreaterThan_Compute
                ),
            new BinaryOp(
                SymbolId.GreaterEquals,
                ">=",
                Precedence.Compare,
                InequalyCompare_DeriveType,
                GreaterEquals_Compute
                ),
            new BinaryOp(
                SymbolId.Times,
                "*",
                Precedence.MulDiv,
                Arithmetic_DeriveType,
                Times_Compute
                ),
            new BinaryOp(
                SymbolId.Divide,
                "/",
                Precedence.MulDiv,
                Arithmetic_DeriveType,
                Divide_Compute
                ),
            new BinaryOp(
                SymbolId.Plus,
                "+",
                Precedence.AddSub,
                Arithmetic_DeriveType,
                Plus_Compute
                ),
            new BinaryOp(
                SymbolId.Minus,
                "-",
                Precedence.AddSub,
                Arithmetic_DeriveType,
                Minus_Compute
                ),
            new BinaryOp(
                SymbolId.And,
                "&&",
                Precedence.AndOr,
                AndOr_DeriveType,
                And_Compute
                ),
            new BinaryOp(
                SymbolId.Or,
                "||",
                Precedence.AndOr,
                AndOr_DeriveType,
                Or_Compute
                ),
        };

        static BinaryOp[] m_table = MakeLookupTable();

        static BinaryOp[] MakeLookupTable()
        {
            var table = new BinaryOp[(int)SymbolId.MAX_VALUE + 1];
            foreach (var op in m_ops)
            {
                int index = (int)op.SymbolId;
                Debug.Assert(table[index] == null);
                table[index] = op;
            }
            return table;
        }

        public static BinaryOp? GetOp(SymbolId symbol)
        {
            return m_table[(int)symbol];
        }

        static TypeDef EqualyCompare_DeriveType(TypeDef type1, TypeDef type2)
        {
            // Equality comparison ('==' or '!=') is possible for two values of the same
            // or between any value and the null value.
            bool canCompare = 
                type1 == type2 ||
                type1 == Types.Null ||
                type2 == Types.Null;

            return canCompare ? Types.Bool : Types.Void;
        }

        static int Equals_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 == arg2 ? 1 : 0;
        }

        static int NotEquals_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 != arg2 ? 1 : 0;
        }

        static TypeDef InequalyCompare_DeriveType(TypeDef type1, TypeDef type2)
        {
            return type1 == Types.Int && type2 == Types.Int ? Types.Bool: Types.Void;
        }

        static int LessThan_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 < arg2 ? 1 : 0;
        }
        static int LessEquals_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 <= arg2 ? 1 : 0;
        }
        static int GreaterThan_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 > arg2 ? 1 : 0;
        }
        static int GreaterEquals_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 >= arg2 ? 1 : 0;
        }

        static TypeDef Arithmetic_DeriveType(TypeDef type1, TypeDef type2)
        {
            return type1 == Types.Int && type2 == Types.Int ? Types.Int : Types.Void;
        }

        static int Times_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 * arg2;
        }
        static int Divide_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg2 != 0 ? arg1 / arg2 : 0;
        }
        static int Plus_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 + arg2;
        }
        static int Minus_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return arg1 - arg2;
        }

        static TypeDef AndOr_DeriveType(TypeDef type1, TypeDef type2)
        {
            return type1 == Types.Bool && type2 == Types.Bool ? Types.Bool : Types.Void;
        }

        static int And_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return (arg1 != 0) && (arg2 != 0) ? 1 : 0;
        }
        static int Or_Compute(GameState game, TypeDef type1, TypeDef type2, int arg1, int arg2)
        {
            return (arg1 != 0) || (arg2 != 0) ? 1 : 0;
        }
    }
}
