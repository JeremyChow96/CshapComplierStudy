using System.Collections.Generic;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax ElseClause { get; }

        public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement,
            ElseClauseSyntax elseClause) :base(syntaxTree)
        {
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
        }
        public override SyntaxKind Kind => SyntaxKind.IfStatement;

        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return IfKeyword;
        //    yield return Condition;
        //    yield return ThenStatement;
        //    if (ElseClause !=null )
        //    {
        //        yield return ElseClause;
        //    }
        //    else
        //    {
        //        yield break; 
        //    } 
        //}

    }
}