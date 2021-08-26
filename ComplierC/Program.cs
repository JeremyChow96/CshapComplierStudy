using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.IO;
using Lib.CodeAnalysis.Symbols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace complier
{
    internal static class Program
    {
        private static void Main(string[] args)
        {

            if (args.Length ==0)
            {
                Console.Error.WriteLine("usage : mc <source-paths>");
            }

            if (args.Length > 1)
            {
                Console.WriteLine("error: only one path supported right now.");
                return;
            }

            var path = args.Single();

            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exists.");
                return;
            }

            var syntaxTree = SyntaxTree.Load(path);

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (result.Diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
            }
            else
            {
                if (result.Value!=null)
                {
                    Console.WriteLine(result.Value);
                }
            }

          


        }
    }
}
