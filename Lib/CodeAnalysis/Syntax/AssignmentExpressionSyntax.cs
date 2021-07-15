using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken, ExpressionSyntax expression)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
        }

        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return Identifier;
        //    yield return EqualToken;
        //    yield return Expression;
        //}
    }

}
