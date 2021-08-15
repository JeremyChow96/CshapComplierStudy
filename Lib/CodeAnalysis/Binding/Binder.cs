using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lib.CodeAnalysis.Lowering;
using Lib.CodeAnalysis.Symbols;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly FunctionSymbol _function;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        private BoundScope _scope;

        public Binder(BoundScope parent, FunctionSymbol function)
        {
            _scope = new BoundScope(parent);
            _function = function;
            
            //if function exists and it has parameter,we need to declare the parameter variable
            if (_function!=null)
            {
                foreach (var parameter in _function.Parameters)
                {
                    _scope.TryDeclareVariable(parameter);
                }
            }
            
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope,null);

            foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            {
                binder.BindFunctionDeclaration(function);
            }

            var statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
            {
                var s = binder.BindStatement(globalStatement.Statement);
                statementBuilder.Add(s);
            }

            var statement = new BoundBlockStatement(statementBuilder.ToImmutable());

            var functions = binder._scope.GetDeclaredFunctions();
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();
            if (previous != null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }

            return new BoundGlobalScope(previous, diagnostics, functions,variables, statement);
        }

        /// <summary>
        /// Register function's bodies. walk through the whole scope (multiple submissions)
        /// </summary>
        /// <param name="globalScope"></param>
        /// <returns></returns>
        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = new DiagnosticBag();


            // walk through the whole scope! make sure the body in  two different submissions!
            var scope = globalScope;
            while (scope!=null)
            {
                foreach (var function in scope.Functions)
                {
                    var binder = new Binder(parentScope, function);
                    var body = binder.BindStatement(function.Declaration.Body);
                    var loweredBody = Lowerer.Lower(body);
                    functionBodies.Add(function,loweredBody);

                    diagnostics.AddRange(binder.Diagnostics);
                }

                scope = scope.Previous;
            }
            
        

            var boundProgram = new BoundProgram(globalScope, functionBodies.ToImmutable(), diagnostics);
            return boundProgram;
        }
        
        
        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            var seenParameterNames = new HashSet<string>();
            foreach (var parameterSyntax in syntax.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span,parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            // for function result type
            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
            if (type !=TypeSymbol.Void)
            {
                _diagnostics.XXX_ReportFunctionAreUnsupported(syntax.Type.Span);
            }

            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type,syntax);
            if ( !_scope.TryDeclareFunction(function))
            {
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);
            }

           

        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                
                foreach (var function in previous.Functions)
                {
                    scope.TryDeclareFunction(function);
                }
                
                foreach (var v in previous.Variable)
                {
                    scope.TryDeclareVariable(v);
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result=  new BoundScope(null);
            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }

            return result;
        }

        public DiagnosticBag Diagnostics => _diagnostics;


        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax) syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax) syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax) syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax) syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax) syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatmentSyntax) syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindForStatement(ForStatmentSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var identifier = syntax.Identifier;
            var variable = BindVariable(identifier, true, TypeSymbol.Int);
            var body = BindStatement(syntax.Body);
            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }


        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;
            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            //check if the target can be converted from the initializer's type 
            var convertedInitializer = BindConversion(syntax.Initializer.Span,initializer,variableType);
        
            var variable = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);
            
            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax ==null)
            {
                return null;
            }

            var type = LookupType(syntax.Identifier.Text);
            if (type ==null)
            {
                _diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);
                return null;
            }

            return type;
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var statement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, statement, elseStatement);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression,canBeVoid:true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion( syntax,targetType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax) syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax) syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax) syntax);
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax) syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax) syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax) syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax) syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            {
                return BindConversion(syntax.Arguments[0], type,allowExplicit:true);
            }
            

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();
            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }
            
            
            if (!_scope.TryLookupFunction(syntax.Identifier.Text,out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongArguementCount(syntax.Span,function.Name,function.Parameters.Length,syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                var boundArgument = boundArguments[i];
                var parameter = function.Parameters[i];

                // parameter type check
                if (boundArgument.Type != parameter.Type)
                {
                    _diagnostics.ReportWrongArguementType(syntax.Arguments[i].Span,parameter.Name,parameter.Type,boundArgument.Type);
                    return new BoundErrorExpression();
                }
            }
            
            // _diagnostics.ReportBadCharater(syntax.Identifier.Span.Start,'X');
            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }


        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported an error so we can just return an error expression
                return new BoundErrorExpression();
            }

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }


            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var boundExpresion = BindExpression(syntax.Expression);


            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.Identifier.Span, name);
                return boundExpresion;
            }

            if (variable.IsReadOnly)
            {
                _diagnostics.ReportCannotAssign(syntax.EqualToken.Span, name); 
            }

            var convertedExpression = BindConversion(syntax.Expression.Span, boundExpresion, variable.Type);

            return new BoundAssignmentExpression(variable, convertedExpression);
        }
        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                    boundOperand.Type);
                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }


        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            if (boundLeft.Type == TypeSymbol.Error ||
                boundRight.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundBinaryOpertor.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text,
                    boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }


        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;

            VariableSymbol variable = _function == null
                ? new GlobalVariableSymbol(name, isReadOnly, type)
                : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
            {
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);
            }

            return variable;
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax,TypeSymbol type ,bool allowExplicit =false)
        {
            var expression = BindExpression(syntax);
            var diagnosticSpan = syntax.Span;

            return BindConversion(diagnosticSpan, expression, type,allowExplicit);
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan,BoundExpression expression, TypeSymbol type ,bool allowExplicit =false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exist)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    _diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);
                }
                return new BoundErrorExpression();
            }

            if (!allowExplicit &&conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
            }
            
            if (conversion.IsIdentity)
            {
                return expression;
            }

            return new BoundConversionExpression(type, expression);
        }

        private TypeSymbol LookupType(string name)
        {
            switch (name)
            {
                case "bool": return TypeSymbol.Bool;
                case "int": return TypeSymbol.Int;
                case "string": return TypeSymbol.String;
                default: return null;
            }
        }
    }
}