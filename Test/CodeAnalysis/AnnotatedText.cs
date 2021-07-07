using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using complier.CodeAnalysis;

namespace Test.CodeAnalysis
{
    internal sealed class AnnotatedText
    {
        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            var textBuilder = new StringBuilder();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            var starstack = new Stack<int>();

            var position = 0;


            foreach (var c in text)
            {
                if (c == '[')
                {
                    starstack.Push(position);
                }
                else if (c == ']')
                {
                    if (starstack.Count == 0)
                    {
                        throw new ArgumentException("Too many ']' in text", nameof(text));
                    }

                    var star = starstack.Pop();
                    var end = position;
                    var span = TextSpan.FromBounds(star, end);
                    spanBuilder.Add(span);
                }
                else
                {
                    position++;
                    textBuilder.Append(c);
                }
            }

            if (starstack.Count != 0)
            {
                throw new ArgumentException("Missing  ']' in text", nameof(text));
            }


            return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutable());
        }

        private static string Unindent(string text)
        {
            var lines = UnindentLines(text);

            return string.Join(Environment.NewLine, lines);
        }

        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            var minIndentation = int.MaxValue;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Trim().Length == 0)
                    continue;

                var indentataion = line.Length - line.TrimStart().Length;
                minIndentation = Math.Min(minIndentation, indentataion);
            }


            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Length == 0)
                    continue;
                
                lines[i] = lines[i].Substring(minIndentation);
               // lines[i] = lines[i].TrimStart();
            }

            while (lines.Count > 0 && lines[0].Trim().Length == 0)
                lines.RemoveAt(0);

            // lines.Count-1
            while (lines.Count > 0 && lines[^1].Trim().Length == 0)
                lines.RemoveAt(lines.Count - 1);
            return lines.ToArray();
        }
    }
}