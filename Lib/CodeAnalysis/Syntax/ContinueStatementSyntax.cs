using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    internal class ContinueStatementSyntax : StatementSyntax
    {
     
        public ContinueStatementSyntax(SyntaxToken keyword)
        {
            Keyword = keyword;
        }

        public SyntaxToken Keyword { get; }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
    }
}