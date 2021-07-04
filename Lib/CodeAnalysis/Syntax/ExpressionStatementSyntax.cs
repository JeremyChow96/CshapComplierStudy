using System.Collections.Generic;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionSyntax Expression { get; }

        //
        public ExpressionStatementSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return Expression;
        //}
    }
}