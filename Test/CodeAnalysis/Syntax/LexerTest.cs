using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Test.CodeAnalysis.Syntax

{
    public class LexerTest
    {
        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Tokens(SyntaxKind kind, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text,
            SyntaxKind t2Kind,string t2Text)
        {
            var text = t1Text + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();
             
             Assert.Equal(2,tokens.Length);

            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);

            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[1].Text, t2Text);
      
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairs_WithSeparator(SyntaxKind t1Kind, string t1Text,
                                           SyntaxKind separatorKind, string separatorText,
                                           SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text +  separatorText +t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(3, tokens.Length);

            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);

            Assert.Equal(tokens[1].Kind, separatorKind);
            Assert.Equal(tokens[1].Text, separatorText);

            Assert.Equal(tokens[2].Kind, t2Kind);
            Assert.Equal(tokens[2].Text, t2Text);

        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var t in GetTokens().Concat(GetSeparator()))
            {
                yield return new object[] { t.kind, t.text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var t in GetTokenPairs())
            {
                yield return new object[] { t.t1kind, t.t1Text, t.t2kind, t.t2text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var t in GetTokenPairsWithSeparator())
            {
                yield return new object[] { t.t1kind, t.t1Text, t.separatorKind,t.separatorText, t.t2kind, t.t2text };
            }
        }
        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            return new[]
            {
                (SyntaxKind.literalToken,"1") ,
                (SyntaxKind.literalToken,"123") ,
                (SyntaxKind.PlusToken,"+") ,
                (SyntaxKind.MinusToken,"-") ,
                (SyntaxKind.StarToken,"*") ,
                (SyntaxKind.SlashToken,"/") ,
                (SyntaxKind.BangToken,"!") ,
                (SyntaxKind.EqualsToken,"=") ,
                (SyntaxKind.PipePipeToken,"||") ,
                (SyntaxKind.AmpersandAmpersandToken,"&&") ,
                (SyntaxKind.EqualsEqualsToken,"==") ,
                (SyntaxKind.BangEqualsToken,"!=") ,
                (SyntaxKind.OpenParenthesisToken,"(") ,
                (SyntaxKind.CloseParentesisToken,")") ,
                (SyntaxKind.IdentifierToken,"a"),
                (SyntaxKind.IdentifierToken,"abc"),
                (SyntaxKind.TrueKeyword,"true") ,
                (SyntaxKind.FalseKeyword,"false") ,
               
            };

        }
        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparator()
        {
            return new[]
            {
               (SyntaxKind.WhitespaceToken," "),
               (SyntaxKind.WhitespaceToken,"\r"),
               (SyntaxKind.WhitespaceToken,"\n"),
               (SyntaxKind.WhitespaceToken,"\r\n"),
            };

        }

        private static bool RequiresSeparator(SyntaxKind t1Kind,SyntaxKind t2Kind)
        {
            //TrueKeyword or FalseKeyword
            var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
            var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");

            if (t1Kind ==SyntaxKind.IdentifierToken &&t2Kind== SyntaxKind.IdentifierToken)
            {
                return true;
            }

            if (t1Kind == SyntaxKind.literalToken && t2Kind == SyntaxKind.literalToken)
            {
                return true;
            }

            if (t1IsKeyword && t2IsKeyword)
            {
                return true;
            }

            // falseabc why this is ok?
            if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
            {
                return true;
            }

            // abcture
            //
            if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
            {
                return true;
            }
          

            if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
            {
                return true;
            }

            if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            {
                return true;
            }

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            {
                return true;
            }

            //TODO


            return false;
        }



        private static IEnumerable<(SyntaxKind t1kind, string t1Text, SyntaxKind t2kind, string t2text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiresSeparator(t1.kind,t2.kind))
                    {
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                    }
                    
                }
            }
        }

        private static IEnumerable<(SyntaxKind t1kind, string t1Text,SyntaxKind separatorKind,string separatorText ,SyntaxKind t2kind, string t2text)> GetTokenPairsWithSeparator()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.kind, t2.kind))
                    {
                        foreach (var s in GetSeparator())
                        {
                            yield return (t1.kind, t1.text,s.kind,s.text, t2.kind, t2.text);

                        }
                    }

                }
            }
        }
    }
}