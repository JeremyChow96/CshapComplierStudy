namespace complier
{
   public  enum SyntaxKind
    {
        //Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlahToken,
        OpenParenthesisToken,
        CloseParentesisToken,

        //Expressions
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression
    }

}
