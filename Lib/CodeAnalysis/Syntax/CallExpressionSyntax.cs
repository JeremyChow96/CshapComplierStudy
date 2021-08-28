using System.Collections.Immutable;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    // print("hello")
    // add(1,2)
    public  sealed class  CallExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }


        public CallExpressionSyntax(SyntaxTree syntaxTree,
                                    SyntaxToken identifier,
                                    SyntaxToken openParenthesisToken,
                                    SeparatedSyntaxList<ExpressionSyntax> arguments,
                                    SyntaxToken closeParenthesisToken)
            :base(syntaxTree)
        {
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Arguments = arguments;
            CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;
    }
}