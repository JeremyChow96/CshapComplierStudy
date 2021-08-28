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

        public void ReportInvalidNumber(TextLocation loaction, string text, TypeSymbol type)
        {
            var message = $"The number {text} isn't valid {type}.";
            Report(loaction, message);
        }

        public void ReportBadCharater(TextLocation location, char current)
        {

            var message = $"Bad character input: '{current}'.";
            Report(location, message);

        }
        public void ReportUnterminatedString(TextLocation loaction)
        {
            var message = $"Unterminated string literal.";
            Report(loaction, message);
        }

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }



        public void ReportUnexpectedToken(TextLocation loaction, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(loaction, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation loaction, string text, TypeSymbol type)
        {
            var message = $"Unary operator '{text}' is not defined for type '{type}'.";
            Report(loaction, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation loaction, string text, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary operator '{text}' is not defined for type '{leftType}' and '{rightType}'.";
            Report(loaction, message);
        }

        public void ReportUndefinedVariable(TextLocation loaction, string name)
        {
            var message = $"Variable '{name}' doesn't exist.";
            Report(loaction, message);

        }

        public void ReportNotAVariable(TextLocation loaction, string name)
        {
            var message = $"'{name}' is not a variable.";
            Report(loaction, message);
        }

        public void ReportUndefinedType(TextLocation loaction, string name)
        {
            var message = $"Type '{name}' doesn't exist.";
            Report(loaction, message);
        }

        public void ReportNotAFunction(TextLocation loaction, string name)
        {
            var message = $"'{name}' is not a function.";
            Report(loaction, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation loaction, string name)
        {
            var message = $"'{name}' is already declared.";
            Report(loaction, message);
        }
        public void ReportParameterAlreadyDeclared(TextLocation loaction, string parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' already exists.";
            Report(loaction, message);
        }



        public void ReportCannotConvert(TextLocation loaction, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(loaction, message);
        }
        public void ReportCannotConvertImplicitly(TextLocation loaction, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot implicitly convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(loaction, message);
        }
        public void ReportCannotAssign(TextLocation loaction, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(loaction, message);
        }



        public void ReportUndefinedFunction(TextLocation loaction, string name)
        {
            var message = $"Function '{name}' doesn't exist.";
            Report(loaction, message);
        }

        public void ReportWrongArguementCount(TextLocation loaction, string name, int expectedCount, int actualCount)
        {
            var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(loaction, message);
        }

        public void ReportWrongArguementType(TextLocation loaction, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
            Report(loaction, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation loaction)
        {
            var message = $"Expression must have a value.";
            Report(loaction, message);
        }


        //public void XXX_ReportFunctionAreUnsupported(TextLocation loaction)
        //{
        //    var message = $"Functions with return values are unsupported.";
        //    Report(loaction, message);
        //}

        public void ReportInvalidBreakOrContinue(TextLocation loaction, string text)
        {
            var message = $"The keyword '{text}' can only be used inside of loops.";
            Report(loaction, message);
        }


        public void ReportAllPathMustReturn(TextLocation loaction)
        {
            var message = "Not all code paths return a value.";
            Report(loaction, message);
        }


        public void ReportInvalidReturn(TextLocation loaction)
        {

            var message = "The 'return' keyword can only be used inside of functions.";
            Report(loaction, message);
        }

        public void ReportInvalidReturnExpression(TextLocation loaction, string functionName)
        {
            var message = $"Since the function '{functionName}' does not return a value. the 'return' keyword cannot be followed by an expression.";
            Report(loaction, message);
        }

        public void ReportMissingReturnExpression(TextLocation loaction, TypeSymbol returnType)
        {
            var message = $"An expression of type '{returnType}' is expected.";
            Report(loaction, message);
        }
    }

}
