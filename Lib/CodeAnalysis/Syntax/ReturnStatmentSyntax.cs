using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class ReturnStatmentSyntax : StatementSyntax
    {
        public ReturnStatmentSyntax(SyntaxToken returnKeyword, ExpressionSyntax expression)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
        }
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }
}