using System.Collections.Generic;

namespace complier.CodeAnalysis.Syntax
{
     public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literalToken)
        {
            LiteralToken = literalToken;
        }

        public SyntaxToken LiteralToken { get; }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }

}
