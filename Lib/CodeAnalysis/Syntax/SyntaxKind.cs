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
        AmpersandToken,
        PipeToken,
        HatToken,
        TildeToken,
        EqualsToken,
        PipePipeToken,
        AmpersandAmpersandToken,
        EqualsEqualsToken,
        BangEqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        IdentifierToken,
        OpenBraceToken,
        CloseBraceToken,
        GreaterToken,
        GreaterOrEqualsToken,
        LessToken,
        LessOrEqualsToken,
        CommaToken,
        StringToken,
        ColonToken,



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
        FunctionKeyword,
        ReturnKeyword,


        //Expressions
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        NameExpression,
        AssignmentExpression,
        CallExpression,


        //nodes
        CompilationUnit,
        ElseClause,
        TypeClause,
        GlobalStatement,
        Parameter,
        FunctionDeclaration,

        //statements
        ExpressionStatement,
        BlockStatement,
        IfStatement,
        WhileStatement,
        VariableDeclaration,
        ForStatement,
        ContinueKeyword,
        BreakKeyword,
        ContinueStatement,
        BreakStatement,
        ReturnStatement,
    }

}
