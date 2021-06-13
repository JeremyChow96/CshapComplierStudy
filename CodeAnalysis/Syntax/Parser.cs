using System.Collections.Generic;

namespace complier.CodeAnalysis.Syntax
{


    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _position;
        private List<string> _diagnostics = new List<string>();
        public IEnumerable<string> Diagnostics => _diagnostics;


        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }


            } while (token.Kind != SyntaxKind.EndOfFileToken);


            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);


        /// <summary>
        /// Return current before the position + 1
        /// </summary>
        /// <returns></returns>
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.Add($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}> ");
            return new SyntaxToken(kind, Current.Position, null, null);

        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);

        }

        private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {

            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }


            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }
                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;

        }


        //private ExpressionSyntax ParseExpression()
        //{
        //    return ParseTerm();
        //}


        //private ExpressionSyntax ParseTerm()
        //{
        //    var left = ParseFactor();
        //    //priority rule


        //    while (Current.Kind == SyntaxKind.PlusToken ||
        //          Current.Kind == SyntaxKind.MinusToken)
        //    {
        //        var operatorToken = NextToken();
        //        var right = ParseFactor();
        //        left = new BinaryExpressionSyntax(left, operatorToken, right);
        //    }
        //    return left;
        //}
        //private ExpressionSyntax ParseFactor()
        //{
        //    var left = ParsePrimaryExpression();
        //    //priority rule

        //    while (Current.Kind == SyntaxKind.StarToken ||
        //          Current.Kind == SyntaxKind.SlahToken)
        //    {
        //        var operatorToken = NextToken();
        //        var right = ParsePrimaryExpression();
        //        left = new BinaryExpressionSyntax(left, operatorToken, right);
        //    }
        //    return left;
        //}

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    {
                        var left = NextToken();
                        var exprssion = ParseExpression();
                        var right = MatchToken(SyntaxKind.CloseParentesisToken);
                        return new ParenthesizedExpressionSyntax(left, exprssion, right);
                    }

                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    {

                        // make sure to meet the endOfFileToken
                        var value = Current.Kind == SyntaxKind.TrueKeyword;
                        var keywordToken = NextToken();
                        return new LiteralExpressionSyntax(keywordToken, value);
                    }
                default:
                    {
                        var numberToken = MatchToken(SyntaxKind.literalToken);
                        return new LiteralExpressionSyntax(numberToken);
                    }
            }

       
        }
    }

}
