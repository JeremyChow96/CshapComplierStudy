using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class ReturnStatmentSyntax : StatementSyntax
    {
        public ReturnStatmentSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax expression) : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
        }
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }
}