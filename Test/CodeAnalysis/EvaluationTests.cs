using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1",1)]
        [InlineData("-1", -1)]
        [InlineData("+1", 1)]
        [InlineData("+1 + 2", 3)]
        [InlineData("+1 - 2", -1)]
        [InlineData("1 * 2", 2)]
        [InlineData("6 / 2", 3)]
        [InlineData("(10)", 10)]
        [InlineData("3 == 3", true)]
        [InlineData("33 == 3", false)]
        [InlineData("33 != 3", true)]
        [InlineData("true == true", true)]
        [InlineData("true == false", false)]
        [InlineData("false == false", true)]
        [InlineData("false != false", false)]
        [InlineData("false == true", false)]

        [InlineData("true", true)]
        [InlineData("!true", false)]
        [InlineData("false", false)]
        [InlineData("!false", true)]
        public void SyntaxFact_GetText_RoundTrips(string text,object expectedReuslt)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compliation = new Compilation(syntaxTree);

            var variables = new Dictionary<VariableSymbol, object>();
            var result = compliation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedReuslt, result.Value);
          
        }
    }
}
