using Lib.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace complier.CodeAnalysis.Syntax
{

    public class SyntaxTree
    {
        //ImmutableArray<Diagnostic> diagnostics, CompliationUnitSyntax root
        private  SyntaxTree(SourceText text)
        {
            var paser = new Parser(text);
            var root = paser.ParseCompilationUnit();
            var diagnostics = paser.Diagnostics.ToImmutableArray();


            Text = text;
            Diagnostics = diagnostics;
            Root = root;
     
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitSyntax Root { get; }
    

        public static SyntaxTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
        
            return Parse(sourceText);
        }

        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            var lexer = new Lexer(text);
            while (true)
            {
                var token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                     break;
                }
                yield return token;
            }
        }
    }

}


