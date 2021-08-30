﻿using complier.CodeAnalysis;
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
        private static int Main(string[] args)
        {
       
 
            if (args.Length ==0)
            {
                Console.Error.WriteLine("usage : mc <source-paths>");
            }

            var paths =  GetFilePaths(args);
            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' doesn't exists.");
                    hasErrors = true;
                    continue;
                }
                var syntaxTree = SyntaxTree.Load(path);
                syntaxTrees.Add(syntaxTree);

            }

            if (hasErrors)
            {
                return 1;
            }


            var compilation = new Compilation(syntaxTrees.ToArray());
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (!result.Diagnostics.Any())
            {
                if (result.Value != null)
                {
                    Console.WriteLine(result.Value);
                }
            }
            else
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
                return 1;
            }
            return 0;
          


        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            var result = new SortedSet<string>();

            foreach (var path in paths)
            {
             
                if (Directory.Exists(path))
                {
                    result.UnionWith(Directory.EnumerateFiles(path, "*.ms", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(path);
                }
            }
            return result;
        }
    }
}
