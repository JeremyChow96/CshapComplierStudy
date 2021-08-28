﻿using System.Diagnostics;
using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;

namespace Lib.CodeAnalysis.Syntax
{
   // [DebuggerDisplay(" {Kind} - {Text} : {Value} ")]
   public sealed  class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text, object value)
            : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }

        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);



        public bool IsMissing => Text == null;
    }

}
