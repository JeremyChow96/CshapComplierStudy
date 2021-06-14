namespace complier.CodeAnalysis.Syntax
{
   public  enum SyntaxKind
    {
        //Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        literalToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        PipePipeToken,
        AmpersandAmpersandToken,
        EqualsEqualsToken,
        BangEqualsToken,
        OpenParenthesisToken,
        CloseParentesisToken,

        IdentifierToken,

        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,

        //Keyword  boolean use
        TrueKeyword,
        FalseKeyword,
        NameExpression,
    }

}
