using complier.CodeAnalysis;
using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace complier
{
    class Program
    {
        // 1 + 2 * 3 

        static void Main(string[] args)
        {

            var showTree = false;
            var variables = new Dictionary<VariableSymbol,object>();
            var textBuilder = new StringBuilder();
            Compilation previous = null;
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.Write(">> ");

                Console.ResetColor();

                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);
               
                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    else if (input == "#showTree")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing parse tree" : "Not showing parse tree");
                        continue;
                    }
                    else if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                    else if (input =="#reset")
                    {
                        previous = null;
                        continue;
                    }
 
                }
                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();              

                var syntaxTree = SyntaxTree.Parse(text);
                if (!isBlank&&syntaxTree.Diagnostics.Any())
                {
                    continue;
                }
                
                var compilation = previous ==null
                    ? new Compilation(syntaxTree)
                    : previous.ContinueWith(syntaxTree);
                
                var result = compilation.Evaluate(variables);


                var diagnostics = result.Diagnostics;
           
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }


                if (!diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();

                    previous = compilation;
                }
                else
                {

                    foreach (var diagnostic in diagnostics)
                    {
                        var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var charater = diagnostic.Span.Start - line.Start + 1; 



                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"({lineNumber}, {charater}) :  ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                        var prefix = syntaxTree.Text.ToString(prefixSpan);
                        var error = syntaxTree.Text.ToString(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = syntaxTree.Text.ToString(suffixSpan) ;

                        Console.Write("    ");
                        Console.Write(prefix);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(error);
                        Console.ResetColor();
                        Console.Write(suffix);
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }

                textBuilder.Clear();
            }
        }

      

    }
}
