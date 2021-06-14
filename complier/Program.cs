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
                }else if(line =="q")
                {
                   return ;
                }
    


                var syntaxTree = SyntaxTree.Parse(line);
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate();

                var diagnostics = result.Diagnostics;
           
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();

                }


                if (!diagnostics.Any())
                {
         
                    Console.WriteLine(result.Value);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in diagnostics)
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
