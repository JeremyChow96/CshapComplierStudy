using System.Collections.Generic;
using System.Collections.Immutable;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseToken { get; }

        public BlockStatementSyntax(SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements,
            SyntaxToken closeToken)
        {
            OpenBraceToken = openBraceToken;
            Statements = statements;
            CloseToken = closeToken;
        }
     
        public override SyntaxKind Kind => SyntaxKind.BlockStatement;

        //public override IEnumerable<SyntaxNode> GetChildren()
        //{
        //    yield return OpenBraceToken;
        //    foreach (var statement in Statements)
        //    {
        //        yield return statement;
        //    }
        //    yield return CloseToken;
            
        //}
    }
}