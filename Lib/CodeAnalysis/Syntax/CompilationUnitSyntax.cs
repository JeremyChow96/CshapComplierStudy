using System.Collections.Generic;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken endOfFileToken)
        {
            Expression = expression;
            EndOfFileToken = endOfFileToken;
        }

        public ExpressionSyntax Expression { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompliationUnit;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return EndOfFileToken;
        }
    }

}
