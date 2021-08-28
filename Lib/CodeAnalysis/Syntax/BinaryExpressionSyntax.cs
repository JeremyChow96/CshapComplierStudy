﻿using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{

    public   sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return Left;
        //    yield return OperatorToken;
        //    yield return Right;
        //}
    }
}
