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
        GreaterToken,
        GreaterOrEqualsToken,
        LessToken,
        LessOrEqualsToken,


        //Keyword  boolean use
        TrueKeyword,
        FalseKeyword,
        LetKeyword,
        VarKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        ForKeyword,
        ToKeyword,

        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        NameExpression,
        AssignmentExpression,


        //nodes
        CompliationUnit,
        ElseClause,


        //statements
        ExpressionStatement,
        BlockStatement,
        IfStatement,
        WhileStatement,
        VariableDeclaration,
        ForStatment,
    }

}
