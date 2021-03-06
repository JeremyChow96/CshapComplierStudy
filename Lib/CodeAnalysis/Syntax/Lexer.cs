using Lib.CodeAnalysis.Text;
using System.Text;
using Lib.CodeAnalysis.Symbols;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly SourceText _text;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();


        private int _position;
        private int _start;
        private object _value;
        private SyntaxKind _kind;



        public DiagnosticBag Diagnostics => _diagnostics;

        public Lexer(SyntaxTree syntaxTree)
        {
            _text = syntaxTree.Text;
            _syntaxTree = syntaxTree;
        }

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }


        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;


            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '*':
                    _kind = SyntaxKind.StarToken;
                    _position++;
                    break;
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _position++;
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    _position++;
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    _position++;
                    break;
                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    _position++;
                    break;
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;
                case '：':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
                    _position++;
                    break;
                case '^':
                    _kind = SyntaxKind.HatToken;
                    _position++;
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;
                case '~':
                    _kind = SyntaxKind.TildeToken;
                    _position++;
                    break;
                case '&':
                    _position++;
                    if (Current != '&')
                    {
                        _kind = SyntaxKind.AmpersandToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                    }

                    break;
                case '|':
                    _position++;
                    if (Current != '|')
                    {
                        _kind = SyntaxKind.PipeToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.PipePipeToken;
                    }

                    break;
                case '!':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.BangToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.BangEqualsToken;
                    }

                    break;
                case '>':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.GreaterToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.GreaterOrEqualsToken;
                    }

                    break;
                case '<':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.LessToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.LessOrEqualsToken;
                    }

                    break;
                case '=':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.EqualsEqualsToken;
                    }

                    break;
                case '"':
                    ReadString();
                    break;
                default:
                    if (char.IsDigit(Current))
                    {
                        ReadNumberToken();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhitespace();
                    }
                    else if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else
                    {
                        var span = new TextSpan(_position, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportBadCharater(location, Current);
                        _position++;
                    }

                    break;
            }


            var length = _position - _start;
            var text = SyntaxFacts.GetText(_kind);
            if (text == null)
            {
                text = _text.ToString(_start, length);
            }

            return new SyntaxToken(_syntaxTree ,_kind, _start, text, _value);
        }

        private void ReadString()
        {
            // "tesr "" dadas
            // tesr " dadas
            _position++;
            var sb = new StringBuilder();
            var done = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '"':
                        if (Lookahead =='"')
                        {
                            sb.Append(Current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current))
                _position++;

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (!int.TryParse(text, out var value))
            {
                var span = new TextSpan(_start, length);
                var location = new TextLocation(_text, span);
                _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int);
            }

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }


        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }
    }
}