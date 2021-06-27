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
        [InlineData("+--1", 1)]
        [InlineData("1 + 2", 3)]
        [InlineData("11 -111", -100)]
        [InlineData("1 * 2", 2)]
        [InlineData("6 / 2", 3)]
        [InlineData("(1 + 3) * 3", 12)]
        [InlineData("var a=12", 12)]
        //[InlineData(" (a=10) * a", 100)]

        [InlineData("true == true", true)]
        [InlineData("true == false", false)]
        [InlineData("false == false", true)]
        [InlineData("false != false", false)]
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
