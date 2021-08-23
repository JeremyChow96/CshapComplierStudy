using complier.CodeAnalysis.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private void Report(TextSpan span,string message)
        {
            var diagonstic = new Diagnostic(span, message);
            _diagnostics.Add(diagonstic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"The number {text} isn't valid {type}.";
            Report(span, message);
        }

        public void ReportBadCharater(int position, char current)
        {
            TextSpan span = new TextSpan(position, 1);
            var message = $"Bad character input: '{current}'.";
            Report(span, message);

        }
        public void ReportUnterminatedString(TextSpan span)
        {
            var message = $"Unterminated string literal.";
            Report(span, message);
        }

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }



        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"Unary operator '{text}' is not defined for type '{type}'.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string text, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary operator '{text}' is not defined for type '{leftType}' and '{rightType}'.";
            Report(span, message);
        }

        public void ReportUndefinedVariable(TextSpan span, string name)
        {
            var message = $"Variable '{name}' doesn't exist.";
            Report(span, message);

        }

        public void ReportNotAVariable(TextSpan span, string name)
        {
            var message = $"'{name}' is not a variable.";
            Report(span, message);
        }

        public void ReportUndefinedType(TextSpan span, string name)
        {
            var message = $"Type '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportNotAFunction(TextSpan span, string name)
        {
            var message = $"'{name}' is not a function.";
            Report(span, message);
        }

        public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"'{name}' is already declared.";
            Report(span, message);
        }
        public void ReportParameterAlreadyDeclared(TextSpan span, string parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' already exists.";
            Report(span, message);
        }
        
        
  
        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }
        public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot implicitly convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(span, message);
        }
        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }

       

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            var message = $"Function '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportWrongArguementCount(TextSpan span, string name, int expectedCount,  int actualCount)
        {
            var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(span, message);
        }

        public void ReportWrongArguementType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            var message = $"Expression must have a value.";
            Report(span, message);
        }


        //public void XXX_ReportFunctionAreUnsupported(TextSpan span)
        //{
        //    var message = $"Functions with return values are unsupported.";
        //    Report(span, message);
        //}

        public void ReportInvalidBreakOrContinue(TextSpan span, string text)
        {
            var message = $"The keyword '{text}' can only be used inside of loops.";
            Report(span, message);
        }


        public void ReportAllPathMustReturn(TextSpan span)
        {
            var message = "Not all code paths return a value.";
            Report(span, message);
        }
        

        public void ReportInvalidReturn(TextSpan span)
        {

            var message = "The 'return' keyword can only be used inside of functions.";
            Report(span, message);
        }

        public void ReportInvalidReturnExpression(TextSpan span,string functionName)
        {
            var message = $"Since the function '{functionName}' does not return a value. the 'return' keyword cannot be followed by an expression.";
            Report(span, message);
        }

        public void ReportMissingReturnExpression(TextSpan span,TypeSymbol returnType)
        {
            var message = $"An expression of type '{returnType}' is expected.";
            Report(span, message);
        }
    }

}
