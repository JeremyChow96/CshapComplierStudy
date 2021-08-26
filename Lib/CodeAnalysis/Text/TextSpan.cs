using Lib.CodeAnalysis.Text;

namespace complier.CodeAnalysis
{
    public struct TextLocation
    {
        public TextLocation(SourceText text, TextSpan span)
        {
            Text = text;
            Span = span;
        }

        public SourceText Text { get; }
        public TextSpan Span { get; }

        public string FileName => Text.FileName;

        public int StartLine => Text.GetLineIndex(Span.Start);
        public int EndLine => Text.GetLineIndex(Span.End);

        public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
        public int EndCharacter => Span.End - Text.Lines[StartLine].Start;


    }


    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }

        public int End => Start + Length;

        public static TextSpan FromBounds(int start,int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }

        public override string ToString() => $"{Start} ... {End}";
    }

}
