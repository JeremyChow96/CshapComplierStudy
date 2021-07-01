using complier.CodeAnalysis.Syntax;
using System;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOpertor
    {
        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type)
            : this(syntaxKind, kind, type, type, type)
        {

        }
        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandType, Type resultType)
    : this(syntaxKind, kind, operandType, operandType, resultType)
        {

        }
        private BoundBinaryOpertor(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        private static BoundBinaryOpertor[] _operators =
        {
            new BoundBinaryOpertor(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,typeof(int)),
            new BoundBinaryOpertor(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Substraction,typeof(int)),
            new BoundBinaryOpertor(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,typeof(int)),
            new BoundBinaryOpertor(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,typeof(int)),

            new BoundBinaryOpertor(SyntaxKind.EqualsEqualsToken,BoundBinaryOperatorKind.Equals,typeof(int),typeof(bool)),
            new BoundBinaryOpertor(SyntaxKind.BangEqualsToken,BoundBinaryOperatorKind.NotEquals,typeof(int),typeof(bool)),


            new BoundBinaryOpertor(SyntaxKind.LessToken,BoundBinaryOperatorKind.LessThan,typeof(int),typeof(bool)),
            new BoundBinaryOpertor(SyntaxKind.LessOrEqualsToken,BoundBinaryOperatorKind.LessThanOrEquals,typeof(int),typeof(bool)),

            new BoundBinaryOpertor(SyntaxKind.GreaterToken,BoundBinaryOperatorKind.GreaterThan,typeof(int),typeof(bool)),
            new BoundBinaryOpertor(SyntaxKind.GreaterOrEqualsToken,BoundBinaryOperatorKind.GreaterThanOrEquals,typeof(int),typeof(bool)),


            new BoundBinaryOpertor(SyntaxKind.AmpersandAmpersandToken,BoundBinaryOperatorKind.LogicalAnd,typeof(bool)),
            new BoundBinaryOpertor(SyntaxKind.PipePipeToken,BoundBinaryOperatorKind.LogicalOr,typeof(bool)),
                        new BoundBinaryOpertor(SyntaxKind.EqualsEqualsToken,BoundBinaryOperatorKind.Equals,typeof(bool)),
            new BoundBinaryOpertor(SyntaxKind.BangEqualsToken,BoundBinaryOperatorKind.NotEquals,typeof(bool)),

        };


        public static BoundBinaryOpertor Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
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
