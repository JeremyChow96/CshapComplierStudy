using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{

    public sealed class ForStatmentSyntax : StatementSyntax
    {
        public ForStatmentSyntax(SyntaxTree syntaxTree,
                                 SyntaxToken forKeyword,
                                 SyntaxToken identifier,
                                 SyntaxToken equalsToken,
                                 ExpressionSyntax lowerBound,
                                 SyntaxToken toKeyword,
                                 ExpressionSyntax upperBound,
                                 StatementSyntax body)
            : base(syntaxTree)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            EqualsToken = equalsToken;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            Body = body;
        }
        public override SyntaxKind Kind => SyntaxKind.ForStatement;

        public SyntaxToken ForKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }
}