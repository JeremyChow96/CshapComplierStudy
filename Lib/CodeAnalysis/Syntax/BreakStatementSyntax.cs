using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    internal class BreakStatementSyntax : StatementSyntax
    {
   

        public BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword) : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;

        public SyntaxToken Keyword { get; }
    }
}