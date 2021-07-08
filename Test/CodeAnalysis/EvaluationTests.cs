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
        [InlineData("1", 1)]
        [InlineData("-1", -1)]
        [InlineData("+--1", 1)]
        [InlineData("1 + 2", 3)]
        [InlineData("11 -111", -100)]
        [InlineData("1 * 2", 2)]
        [InlineData("6 / 2", 3)]
        [InlineData("(1 + 3) * 3", 12)]
        [InlineData("var a=12", 12)]
        [InlineData(" { var a = 10  a = a * a}", 100)]

        [InlineData("3 > 4 ", false)]
        [InlineData("2 >= 2", true)]
        [InlineData("6 <= 3", false)]
        [InlineData("2 < 8", true)]
        [InlineData("2 <= 2", true)]
        [InlineData("(2 * 9) <= 6", false)]
        [InlineData("(2 * 9) >= (3 * 9)", false)]

        [InlineData("true == true", true)]
        [InlineData("true == false", false)]
        [InlineData("false == false", true)]
        [InlineData("false != false", false)]
        [InlineData("true", true)]       
        [InlineData("true&&false", false)]
        [InlineData("true&&true", true)]
        [InlineData("true||false", true)]
        [InlineData("false||false", false)]

        [InlineData("!true", false)]
        [InlineData("false", false)]
        [InlineData("!false", true)]

        [InlineData(" { var a = 0 if a == 0 a = 10  a}", 10)]
        [InlineData(" { var a = 0 if a == 4 a = 10  a}", 0)]
        [InlineData(" { var a = 0 if a == 0 a = 10 else a = 5 a}", 10)]
        [InlineData(" { var a = 0 if a == 4 a = 10 else a = 5 a}", 5)]
        [InlineData(" { var i = 10  var result = 0  while i> 0 {result = result + i i = i - 1}   result }", 55)]
        [InlineData(" { var result = 0  for i = 1 to 10  result = result + i }", 55)]

        public void SyntaxFact_GetText_RoundTrips(string text, object expectedReuslt)
        {
            AssertValue(text, expectedReuslt);
        }

      
        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
                ";

            var diagnostics = @"Variable 'x' is already declared.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_BlockStatement_Reports_NoInfiniteLoop()
        {
            var text = @"
                {
                [)][]
            ";

            var diagnostics = @"
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }


        [Fact]
        public void Evaluator_NameExpression_Reports_Undefined()
        {
            var text = @"[x] * 10";
            
            var diagnostics = @"Variable 'x' doesn't exist.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
        {
            var text = @"[]";

            var diagnostics = @"Unexpected token <EndOfFileToken>, expected <IdentifierToken>.";

            AssertDiagnostics(text, diagnostics);
        }



        [Fact]
        public void Evaluator_AssignmentExpression_Reports_Undefined()
        {
            var text = @"{
                             [x] = 10                 
                        }"
                ;
            
            var diagnostics = @"Variable 'x' doesn't exist.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotAssign()
        {
            var text = @"{
                            let x = 10
                            x [=] 15
                        }"
                ;

            var diagnostics = @"Variable 'x' is read-only and cannot be assigned to.";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotConvert()
        {
            var text = @"{
                            var x = 10
                            x = [true]
                        }"
                ;
            
            var diagnostics = @"Cannot convert type 'System.Boolean' to 'System.Int32'.";

            AssertDiagnostics(text, diagnostics);
        }


        [Fact]
        public void Evaluator_IfStatement_Reports_CannotConvert()
        {
            var text = @"{
                            var x = 10
                            if [10]
                                x = 12
                        }"
                ;

            var diagnostics = @"Cannot convert type 'System.Int32' to 'System.Boolean'.";

            AssertDiagnostics(text, diagnostics);
        }


        [Fact]
        public void Evaluator_WhileStatement_Reports_CannotConvert()
        {
            var text = @"{
                            var x = 10
                            while [10]
                                x = 12
                        }"
                ;

            var diagnostics = @"Cannot convert type 'System.Int32' to 'System.Boolean'.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert()
        {
            var text = @"{
                            var r = 0
                            for i = [false] to 10 
                                 r =  r + i
                        }"
                ;

            var diagnostics = @"Cannot convert type 'System.Boolean' to 'System.Int32'.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            var text = @"[+]true";
            
            var diagnostics = @"Unary operator '+' is not defined for type 'System.Boolean'.";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            var text = @"12 [*] true";
            
            var diagnostics = @"Binary operator '*' is not defined for type 'System.Int32' and 'System.Boolean'.";

            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedReuslt)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compliation = new Compilation(syntaxTree);

            var variables = new Dictionary<VariableSymbol, object>();
            var result = compliation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedReuslt, result.Value);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

      

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");

            var diagnostics = result.Diagnostics;
            Assert.Equal(expectedDiagnostics.Length, diagnostics.Length);


            for (int i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = result.Diagnostics[i].Message;

                Assert.Equal(expectedMessage, actualMessage);
                
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Span;
                Assert.Equal(expectedSpan, actualSpan);

            }
        }
    }
}