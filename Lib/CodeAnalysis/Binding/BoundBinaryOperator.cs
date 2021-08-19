using complier.CodeAnalysis.Syntax;
using System;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOpertor
    {
        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
            : this(syntaxKind, kind, type, type, type)
        {
        }

        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType,
            TypeSymbol resultType)
            : this(syntaxKind, kind, operandType, operandType, resultType)
        {
        }

        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType,
            TypeSymbol rightType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol ResultType { get; }

        private static BoundBinaryOpertor[] _operators =
        {
            new BoundBinaryOpertor(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int),
            new BoundBinaryOpertor(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Substraction, TypeSymbol.Int),
            new BoundBinaryOpertor(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int),
            new BoundBinaryOpertor(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int),

            new BoundBinaryOpertor(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int),
            new BoundBinaryOpertor(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int),
            new BoundBinaryOpertor(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int),


            new BoundBinaryOpertor(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Int,
                TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Int,
                TypeSymbol.Bool),




            new BoundBinaryOpertor(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Int,
                TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessThanOrEquals,
                TypeSymbol.Int, TypeSymbol.Bool),

            new BoundBinaryOpertor(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Int,
                TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEquals,
                TypeSymbol.Int, TypeSymbol.Bool),

            new BoundBinaryOpertor(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),

            new BoundBinaryOpertor(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd,
                TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool),

            new BoundBinaryOpertor(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),
            new BoundBinaryOpertor(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.String,TypeSymbol.Bool),
            new BoundBinaryOpertor(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.String,TypeSymbol.Bool),
        };


        public static BoundBinaryOpertor Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind &&
                    op.LeftType == leftType &&
                    op.RightType == rightType)
                {
                    return op;
                }
            }

            return null;
        }
    }
}