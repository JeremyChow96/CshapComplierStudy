using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Test.CodeAnalysis.Syntax
{

    public class LexerTest
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedStringLiteral()
        {

            var Text = "\"test";
            var tokens = SyntaxTree.ParseTokens(Text, out var diagnostics); 

            var token = Assert.Single(tokens);


            Assert.Equal(SyntaxKind.StringToken, token.Kind);
            Assert.Equal(Text, token.Text);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 1), diagnostic.Span);
            Assert.Equal("Unterminated string literal.", diagnostic.Message);

        }



        [Fact]
        public void Lexer_Covers_AllTokens()
        {
            var tokenKinds = Enum.GetValues<SyntaxKind>().
                Where(k => k.ToString().EndsWith("Keyword") || k.ToString().EndsWith("Token")).
                ToList();

            var testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(c => c.kind);

            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.ExceptWith(testedTokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.BadToken);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
          //  untestedTokenKinds.Remove(SyntaxKind.ColonToken);


            Assert.Empty(untestedTokenKinds);

   
        }

        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
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

            Assert.Equal( t1Kind,tokens[0].Kind);
            Assert.Equal( t1Text,tokens[0].Text);
            Assert.Equal( t2Kind,tokens[1].Kind);
            Assert.Equal( t2Text,tokens[1].Text);
      
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

            Assert.Equal(t1Kind, tokens[0].Kind);
            Assert.Equal(t1Text, tokens[0].Text);

            Assert.Equal(separatorKind, tokens[1].Kind);
            Assert.Equal(separatorText, tokens[1].Text);

            Assert.Equal(t2Kind, tokens[2].Kind);
            Assert.Equal(t2Text, tokens[2].Text);

        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var t in GetTokens().Concat(GetSeparators()))
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
            var fixedTokens = Enum.GetValues<SyntaxKind>()
                .Select(k=>(kind:k,text: SyntaxFacts.GetText(k)))
                .Where(t=>t.text!=null);

            var dynamciToken = new[]
           {
                (SyntaxKind.NumberToken,"1") ,
                (SyntaxKind.NumberToken,"123") ,
                (SyntaxKind.IdentifierToken,"a"),
                (SyntaxKind.IdentifierToken,"abc"),
                (SyntaxKind.StringToken,"\"test\""),
                (SyntaxKind.StringToken,"\"te\"\"st\""),
            };
            return fixedTokens.Concat(dynamciToken);

        }
        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]
            {
               (SyntaxKind.WhitespaceToken," "),
               (SyntaxKind.WhitespaceToken,"   "),
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

            switch (t1Kind)
            {
                case SyntaxKind.IdentifierToken when t2Kind== SyntaxKind.IdentifierToken:
                case SyntaxKind.NumberToken when t2Kind == SyntaxKind.NumberToken:
                    return true;
            }

            switch (t1IsKeyword)
            {
                case true when t2IsKeyword:
                // falseabc why this is ok?
                case true when t2Kind == SyntaxKind.IdentifierToken:
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

            if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken)
            {
                return true;
            }

            if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandAmpersandToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipeToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipePipeToken)
            {
                return true;
            }
            if (t1Kind == SyntaxKind.StringToken && t2Kind == SyntaxKind.StringToken)
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
                        foreach (var s in GetSeparators())
                        {
                            yield return (t1.kind, t1.text,s.kind,s.text, t2.kind, t2.text);

                        }
                    }

                }
            }
        }
    }
}
