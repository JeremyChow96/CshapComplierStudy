﻿using complier;
using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Lib.CodeAnalysis.Symbols;
using Lib.CodeAnalysis.IO;

internal sealed class MinskRepl : Repl
{
    private Compilation _previous;
    private bool _showTree;
    private bool _showProgram;
    private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();


    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens)
        {
            var isKeyword = token.Kind.ToString().EndsWith("Keyword");
            var isNumber = token.Kind == SyntaxKind.NumberToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
            var isString = token.Kind == SyntaxKind.StringToken;
            if (isKeyword)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (isIdentifier)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else if (isNumber)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (isString)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            Console.Write(token.Text);
            Console.ResetColor();
        }
    }

    protected override void EvaluateMetaCommand(string input)
    {
        switch (input)
        {
            case "#showTree":
                _showTree = !_showTree;
                Console.WriteLine(_showTree ? "Showing parse tree" : "Not showing parse tree");

                break;
            case "#showProgram":
                _showProgram = !_showProgram;
                Console.WriteLine(_showProgram ? "Showing bound tree" : "Not showing bound tree");

                break;
            case "#cls":
                Console.Clear();

                break;
            case "#reset":
                _previous = null;
                _variables.Clear();
                ClearHistory();
                break;
            default:
                base.EvaluateMetaCommand(input);
                break;
        }
    }

    protected override void EvaluateSubmission(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);

        var compilation = _previous == null
            ? new Compilation(syntaxTree)
            : _previous.ContinueWith(syntaxTree);

        if (_showTree)
        {
            syntaxTree.Root.WriteTo(Console.Out);
        }

        if (_showProgram)
        {
            compilation.EmitTree(Console.Out);
        }

        var result = compilation.Evaluate(_variables);
        var diagnostics = result.Diagnostics;
        if (!diagnostics.Any())
        {
            if (result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }


            _previous = compilation;
        }
        else
        {
            Console.Error.WriteDiagnostics(result.Diagnostics,syntaxTree);
  
        }
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        var lastTwoLinesAreBlank = text.Split(Environment.NewLine)
            .Reverse()
            .TakeWhile(c => string.IsNullOrEmpty(c))
            .Take(2)
            .Count() == 2;
        if (lastTwoLinesAreBlank)
        {
            return true;
        }


        var syntaxTree = SyntaxTree.Parse(text);
        
        //use member because we need to exclude the EndOfFileToken
        if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
        {
            return false;
        }
        
        // if (syntaxTree.Diagnostics.Any())
        // {
        //     if (syntaxTree.Root.Statement.GetLastToken().IsMissing)
        //     {
        //         return false;
        //     }
        // }

        return true;
    }
}