using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class VariableDeclarationSyntax :StatementSyntax
    {


        // var  x =10 or let  x = 10
        public VariableDeclarationSyntax(SyntaxToken keyword,
            SyntaxToken identifier,
            TypeClauseSyntax typeClause,
            SyntaxToken equalsToken,
            ExpressionSyntax initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return Keyword;
        //    yield return Identifier;
        //    yield return EqualsToken;
        //    yield return Initializer;
        //}
    }
}