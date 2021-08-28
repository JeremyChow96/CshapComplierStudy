﻿using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax operand) : base(syntaxTree)
        {
           
            OperatorToken = operatorToken;
            Operand = operand;
        }
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Operand { get; }

        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
           
        //    yield return OperatorToken;
        //    yield return Operand;
        //}
    }
}
