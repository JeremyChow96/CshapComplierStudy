namespace complier.CodeAnalysis.Syntax
{
    public sealed class ForStatmentSyntax : StatementSyntax
    {
        public ForStatmentSyntax(SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken equals, ExpressionSyntax lowerBound, SyntaxToken toKeyword, ExpressionSyntax upperBound, StatementSyntax body)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            Equals = equals;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            Body = body;
        }
        public override SyntaxKind Kind => SyntaxKind.ForStatment;

        public SyntaxToken ForKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken Equals { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }
}