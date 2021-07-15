using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class VariableDeclarationSyntax :StatementSyntax
    {
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }

        // var  x =10 or let  x = 10
        public VariableDeclarationSyntax(SyntaxToken keyword,
            SyntaxToken identifier,
            SyntaxToken equalsToken,
            ExpressionSyntax initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return Keyword;
        //    yield return Identifier;
        //    yield return EqualsToken;
        //    yield return Initializer;
        //}
    }
}