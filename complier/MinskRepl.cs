﻿using complier;
using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Lib.CodeAnalysis.Syntax;

internal sealed class MinskRepl : Repl
{
    private Compilation _previous;
    private bool _showTree;
    private bool _showProgram;
    private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();


    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in  tokens)
        {
            var isKeyword = token.Kind.ToString().EndsWith("Keyword");
            var isNumber = token.Kind == SyntaxKind.NumberToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(result.Value);
            Console.ResetColor();

            _previous = compilation;
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
                var suffix = syntaxTree.Text.ToString(suffixSpan);

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
        if (syntaxTree.Diagnostics.Any())
        {
            if (syntaxTree.Root.Statement.GetLastToken().IsMissing)
            {
                return false;
            }
        }
        return true;
    }


}

