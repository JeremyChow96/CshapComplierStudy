namespace complier
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
        SlahToken,
        OpenParenthesisToken,
        CloseParentesisToken,

        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
    }

}
