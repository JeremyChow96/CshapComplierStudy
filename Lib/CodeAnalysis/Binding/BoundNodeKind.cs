namespace complier.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        UnaryExpreesion,
        LiteralExpression,
        BinaryExpreesion,
        VariableExpression,
        AssignmentExpression,
        
        
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement
    }
}
