using complier.CodeAnalysis;
using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace complier
{
    partial class Program
    {
        // 1 + 2 * 3 
        
        static void Main(string[] args)
        {
            var showTree = false;

            while (true)
            {

                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;
                if (line =="#showTree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse tree" : "Not showing parse tree");
                    continue;
                }
                else if (line =="#cls")
                {
                    Console.Clear();
                    continue;
                }


                var syntaxTree = SyntaxTree.Parse(line);
                var binder = new Binder();
                var boundExpression = binder.BindExpression(syntaxTree.Root);

                var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();
           
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();

                }


                if (!diagnostics.Any())
                {
                    var e = new Evaluator(boundExpression);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in syntaxTree.Diagnostics)
                    {
                        Console.WriteLine(diagnostic);
                    }
                    Console.ResetColor();

                }


            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool islast = true)
        {
            //├─
            //└─
            //│
            var marker = islast ? "└───" : "├───";

            Console.Write(indent);


            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }
            Console.WriteLine();

            indent += islast ? "    " : "│    ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }

    }
}
