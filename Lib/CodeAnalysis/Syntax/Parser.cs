using Lib.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace complier.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;

        private int _position;


        public DiagnosticBag Diagnostics => _diagnostics;


        public Parser(SourceText text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);


            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
            this._text = text;
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

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var statement = ParseStatement();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(statement, endOfFileToken);
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private StatementSyntax ParseForStatement()
        {
            var keyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var lowerBound = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var upperBound = ParseExpression();
            var body = ParseStatement();

            return new ForStatmentSyntax(keyword, identifier, equals, lowerBound, toKeyword, upperBound, body);
        }

        private StatementSyntax ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(keyword, condition, body);
        }

        private StatementSyntax ParseIfStatement()
        {
            var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(ifKeyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
            {
                return null;
            }
            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);

        }

        private StatementSyntax ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, equals, initializer);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();


            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }

            var clopseBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), clopseBraceToken);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }


        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            #region Comments

            //a + b + 5
            //     +
            //    / \
            //   +   5
            //  / \  
            // a   b
            //
            // a = b = 5
            //     =
            //    / \
            //   a   =
            //      / \
            //     b   5

            #endregion


            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
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
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesisExpression();


                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:

                    return ParseBooleanLiteral();


                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameExpression();
            }
        }

        private ExpressionSyntax ParseParenthesisExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var exprssion = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParentesisToken);
            return new ParenthesizedExpressionSyntax(left, exprssion, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            // make sure to meet the endOfFileToken
            var isTure = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTure ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTure);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }


        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }
    }
}