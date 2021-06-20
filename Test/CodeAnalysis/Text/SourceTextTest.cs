using Lib.CodeAnalysis.Text;
using Xunit;

namespace Test.CodeAnalysis.Syntax
{
    public class SourceTextTest
    {
        [Theory]
        [InlineData(".",1)]
        [InlineData(".\r\n",2)]
        [InlineData(".\r\n\r\n",3)]
        public void SourceText_IncludeLastLine(string text,int expectedLineCount)
        {
            var sourceText = SourceText.From(text);
            Assert.Equal(expectedLineCount, sourceText.Lines.Length);

        }
    }
}
