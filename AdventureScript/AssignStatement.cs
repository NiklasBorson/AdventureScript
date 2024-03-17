﻿using System.Diagnostics;

namespace AdventureLib
{
    sealed class AssignStatement : Statement
    {
        Expr m_leftExpr;
        Expr m_rightExpr;
        bool m_isNewVar;

        public AssignStatement(Expr leftExpr, Expr rightExpr, bool isNewVar)
        {
            m_leftExpr = leftExpr;
            m_rightExpr = rightExpr;
            m_isNewVar = isNewVar;

            // The caller should have already verified that the left-hand expression can be set.
            Debug.Assert(leftExpr.CanSetValue);
            Debug.Assert(CanAssignTypes(leftExpr.Type, rightExpr.Type));
        }

        public static bool CanAssignTypes(TypeDef leftType, TypeDef rightType)
        {
            Debug.Assert(leftType != Types.Void);

            // The types must match or the right-hand expression must be null.
            return leftType == rightType || rightType == Types.Null;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            int value = m_rightExpr.Evaluate(game, frame);
            m_leftExpr.SetValue(game, frame, value);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            if (m_isNewVar)
            {
                writer.Write("var ");
                m_leftExpr.WriteExpr(game, writer);

                if (m_rightExpr.Type == Types.Null)
                {
                    writer.Write($" : {m_leftExpr.Type.Name}");
                }
            }
            else
            {
                m_leftExpr.WriteExpr(game, writer);
            }

            writer.Write(" = ");
            m_rightExpr.WriteExpr(game, writer);
            writer.Write(";");
        }
    }
}
