namespace complier.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        //Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        EqualsToken,
        PipePipeToken,
        AmpersandAmpersandToken,
        EqualsEqualsToken,
        BangEqualsToken,
        OpenParenthesisToken,
        CloseParentesisToken,
        IdentifierToken,
        OpenBraceToken,
        CloseBraceToken,

        //Keyword  boolean use
        TrueKeyword,
        FalseKeyword,
        LetKeyword,
        VarKeyword,

        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        NameExpression,
        AssignmentExpression,


        //nodes
        CompliationUnit,
        
        //statements
        BlockStatement,
        ExpressionStatement,

        VariableDeclaration,
    }

}
