using complier.CodeAnalysis;
using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.CodeAnalysis.Symbols;
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
        [InlineData("var a=12 return a", 12)]
        [InlineData(" { var a = 10  a = a * a return a}", 100)]

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

        [InlineData("1|1", 1)]
        [InlineData("1|2", 3)]  
        [InlineData("true|true", true)]  
        [InlineData("true|false", true)]  
        
        [InlineData("1&1", 1)]
        [InlineData("1&0", 0)]
        [InlineData("true&true", true)]  
        [InlineData("true&false", false)]  
        [InlineData("1|3", 3)]
        [InlineData("1^1", 0)]
        [InlineData("1^2", 3)]
        [InlineData("3^2", 1)]
        [InlineData("true^true", false)]  
        [InlineData("true^false", true)]  
        [InlineData("~3", -4)]  
        [InlineData("\"test\"", "test")]  
        [InlineData("\"te\"\"st\"", "te\"st")]  
        [InlineData("\"test\"==\"test\"",true)]


        [InlineData(" { var a = 0 if a == 0 a = 10 return  a}", 10)]
        [InlineData(" { var a = 0 if a == 4 a = 10 return a}", 0)]
        [InlineData(" { var a = 0 if a == 0 a = 10 else a = 5  return a}", 10)]
        [InlineData(" { var a = 0 if a == 4 a = 10 else a = 5  return a}", 5)]
        [InlineData(" { var i = 10  var result = 0  while i> 0 {result = result + i i = i - 1} return  result }", 55)]
        [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } return result }", 55)]
        [InlineData("{ var a = 10 for i = 1 to (a = a - 1) { } return  a }", 9)]

        public void SyntaxFact_GetText_RoundTrips(string text, object expectedResult)
        {
            AssertValue(text, expectedResult);
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

            var diagnostics = @"'x' is already declared.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Missing()
        {
            var text = @"
                print([)]
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 0.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_InvokeFunctionArguments_Exceeding()
        {
            var text = @"
                print(""Hello""[, "" "", "" world!""])
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 3.
            ";

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
            var text = @"1 + []";

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
            
            var diagnostics = @"Cannot convert type 'bool' to 'int'.";

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

            var diagnostics = @"Cannot convert type 'int' to 'bool'.";

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

            var diagnostics = @"Cannot convert type 'int' to 'bool'.";

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

            var diagnostics = @"Cannot convert type 'bool' to 'int'.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            var text = @"[+]true";
            
            var diagnostics = @"Unary operator '+' is not defined for type 'bool'.";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            var text = @"12 [*] true";
            
            var diagnostics = @"Binary operator '*' is not defined for type 'int' and 'bool'.";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Void_Function_Should_Not_Return_Value()
        {
            var text = @"
                function test()
                {
                    return [1]
                }
            ";

            var diagnostics = @"
                Since the function 'test' does not return a value. the 'return' keyword cannot be followed by an expression.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Function_With_ReturnValue_Should_Not_Return_Void()
        {
            var text = @"
                function test(): int
                {
                    [return]
                }
            ";

            var diagnostics = @"
                An expression of type 'int' is expected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Not_All_Code_Paths_Return_Value()
        {
            var text = @"
                function [test](n: int): bool
                {
                    if (n > 10)
                       return true
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Variables_Can_Shadow_Functions()
        {
            var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

            var diagnostics = @"
                'print' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);

        }

        [Fact]
        public void Evaluator_Expression_Must_Have_Value()
        {
            var text = @"
                function test(n: int)
                {
                    return
                }
                let value = [test(100)]
            ";

            var diagnostics = @"
                Expression must have a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Theory]
        [InlineData("[break]", "break")]
        [InlineData("[continue]", "continue")]
        public void Evaluator_Invalid_Break_Or_Continue(string text, string keyword)
        {
            var diagnostics = $@"
                The keyword '{keyword}' can only be used inside of loops.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Script_Return()
        {
            var text = @"
                return
            ";

        

            AssertValue(text, "");
        }

        [Fact]
        public void Evaluator_Parameter_Already_Declared()
        {
            var text = @"
                function sum(a: int, b: int, [a: int]): int
                {
                    return a + b + c
                }
            ";

            var diagnostics = @"
                A parameter with the name 'a' already exists.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Function_Must_Have_Name()
        {
            var text = @"
                function [(]a: int, b: int): int
                {
                    return a + b
                }
            ";

            var diagnostics = @"
                Unexpected token <OpenParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Wrong_Argument_Type()
        {
            var text = @"
                function test(n: int): bool
                {
                    return n > 10
                }
                let testValue = ""string""
                test([testValue])
            ";

            var diagnostics = @"
               Cannot implicitly convert type 'string' to 'int'. An explicit conversion exists (are you missing a cast?)
            ";

            AssertDiagnostics(text, diagnostics);
        }


        [Fact]
        public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        {
            var text = @"[print] = 42";

            var diagnostics = @"
                'print' is not a variable.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_CallExpression_Reports_Undefined()
        {
            var text = @"[foo](42)";

            var diagnostics = @"
                Function 'foo' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_NotAFunction()
        {
            var text = @"
                {
                    let foo = 42
                    [foo](42)
                }
            ";

            var diagnostics = @"
                'foo' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);
        }



        [Fact]
        public void Evaluator_Bad_Type()
        {
            var text = @"
                function test(n: [invalidtype])
                {
                }
            ";

            var diagnostics = @"
                Type 'invalidtype' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }


        private static void AssertValue(string text, object expectedReuslt)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compliation =  Compilation.CreateScripts(null,syntaxTree);

            var variables = new Dictionary<VariableSymbol, object>();
            var result = compliation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedReuslt, result.Value);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = Compilation.CreateScripts(null, syntaxTree);
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
                var actualSpan = result.Diagnostics[i].Location.Span;
                Assert.Equal(expectedSpan, actualSpan);

            }
        }
    }
}