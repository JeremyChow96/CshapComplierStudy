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
        OpenParenthesisToken,
        CloseParentesisToken,

        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,

        //Keyword  boolean use
        TrueKeyword,
        FalseKeyword,


        IdentifierToken,
        BangToken,
        PipePipeToken,
        AmpersandAmpersandToken,
    }

}
