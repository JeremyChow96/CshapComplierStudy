using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Symbols;
using System.Collections;
using System.Collections.Generic;

namespace complier.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private void Report(TextLocation location, string message)
        {
            var diagonstic = new Diagnostic(location, message);
            _diagnostics.Add(diagonstic);
        }

        public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
        {
            var message = $"The number {text} isn't valid {type}.";
            Report(location, message);
        }

        public void ReportBadCharater(TextLocation location, char current)
        {

            var message = $"Bad character input: '{current}'.";
            Report(location, message);

        }
        public void ReportUnterminatedString(TextLocation location)
        {
            var message = $"Unterminated string literal.";
            Report(location, message);
        }

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }



        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string text, TypeSymbol type)
        {
            var message = $"Unary operator '{text}' is not defined for type '{type}'.";
            Report(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string text, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary operator '{text}' is not defined for type '{leftType}' and '{rightType}'.";
            Report(location, message);
        }

        public void ReportUndefinedVariable(TextLocation location, string name)
        {
            var message = $"Variable '{name}' doesn't exist.";
            Report(location, message);

        }

        public void ReportNotAVariable(TextLocation location, string name)
        {
            var message = $"'{name}' is not a variable.";
            Report(location, message);
        }

        public void ReportUndefinedType(TextLocation location, string name)
        {
            var message = $"Type '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportNotAFunction(TextLocation location, string name)
        {
            var message = $"'{name}' is not a function.";
            Report(location, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation location, string name)
        {
            var message = $"'{name}' is already declared.";
            Report(location, message);
        }
        public void ReportParameterAlreadyDeclared(TextLocation location, string parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' already exists.";
            Report(location, message);
        }



        public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(location, message);
        }
        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot implicitly convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(location, message);
        }
        public void ReportCannotAssign(TextLocation location, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(location, message);
        }



        public void ReportUndefinedFunction(TextLocation location, string name)
        {
            var message = $"Function '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportWrongArguementCount(TextLocation location, string name, int expectedCount, int actualCount)
        {
            var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(location, message);
        }

        public void ReportWrongArguementType(TextLocation location, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
            Report(location, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            var message = $"Expression must have a value.";
            Report(location, message);
        }


        //public void XXX_ReportFunctionAreUnsupported(TextLocation location)
        //{
        //    var message = $"Functions with return values are unsupported.";
        //    Report(location, message);
        //}

        public void ReportInvalidBreakOrContinue(TextLocation location, string text)
        {
            var message = $"The keyword '{text}' can only be used inside of loops.";
            Report(location, message);
        }


        public void ReportAllPathMustReturn(TextLocation location)
        {
            var message = "Not all code paths return a value.";
            Report(location, message);
        }


        public void ReportInvalidReturn(TextLocation location)
        {

            var message = "The 'return' keyword can only be used inside of functions.";
            Report(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            var message = $"Since the function '{functionName}' does not return a value. the 'return' keyword cannot be followed by an expression.";
            Report(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
        {
            var message = $"An expression of type '{returnType}' is expected.";
            Report(location, message);
        }

        public void ReportInvalidExpressionStatement(TextLocation location)
        {
            var message = "Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement.";
            Report(location, message);
        }
    }

}
