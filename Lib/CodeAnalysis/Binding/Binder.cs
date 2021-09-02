using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lib.CodeAnalysis.Lowering;
using Lib.CodeAnalysis.Symbols;
using Lib.CodeAnalysis.Syntax;
using System.Linq;

namespace complier.CodeAnalysis.Binding
{

    internal sealed class Binder
    {
        private readonly bool _isScript;
        private readonly FunctionSymbol _function;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        public Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();

        private BoundScope _scope;
        private int _labelCounter;

        public Binder(bool isScript, BoundScope parent, FunctionSymbol function)
        {
            _scope = new BoundScope(parent);
            _isScript = isScript;
            _function = function;

            //if function exists and it has parameter,we need to declare the parameter variable
            if (_function != null)
            {
                foreach (var parameter in _function.Parameters)
                {
                    _scope.TryDeclareVariable(parameter);
                }
            }

        }

        public static BoundGlobalScope BindGlobalScope(bool isScript, BoundGlobalScope previous, ImmutableArray<SyntaxTree> syntaxTrees)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(isScript, parentScope, null);

            var functionDeclarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                                  .OfType<FunctionDeclarationSyntax>();

            foreach (var function in functionDeclarations)
            {
                binder.BindFunctionDeclaration(function);
            }

            var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members)
                                              .OfType<GlobalStatementSyntax>();


            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var globalStatement in globalStatements)
            {
                var statement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(statement);
            }
            // check global statements     
            var firstGlobalStatementPerSyntaxTree = syntaxTrees.Select(st => st.Root.Members.OfType<GlobalStatementSyntax>().FirstOrDefault())
                                                               .Where(g => g != null)
                                                               .ToArray();


            if (firstGlobalStatementPerSyntaxTree.Length > 1)
            {
                foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                    binder.Diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement.Location);
            }

            var functions = binder._scope.GetDeclaredFunctions();

            FunctionSymbol scriptFunction;
            FunctionSymbol mainFunction;

            if (isScript)
            {
                mainFunction = null;
                if (globalStatements.Any())
                    scriptFunction = new FunctionSymbol("$eval", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Any, null);
                else
                    scriptFunction = null;
            }
            else
            {
                scriptFunction = null;
                mainFunction = functions.FirstOrDefault(f => f.Name == "main");

                if (mainFunction != null)
                {
                    if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                    {
                        binder.Diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.Declaration.Identifier.Location);
                    }

                }

                if (globalStatements.Any())
                {
                    if (mainFunction != null)
                    {
                        binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration.Identifier.Location);

                        foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                        {
                            binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement.Location);
                        }
                    }
                    else
                    {
                        //Call main function
                        mainFunction = new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void, null);

                    }

                }
            }

          

            var diagnostics = binder.Diagnostics.ToImmutableArray();

            var variables = binder._scope.GetDeclaredVariables();
        
            if (previous != null)
            {
                diagnostics= diagnostics.InsertRange(0, previous.Diagnostics);
            }

            return new BoundGlobalScope(previous,
                                        diagnostics,
                                        mainFunction,
                                        scriptFunction,
                                        functions,
                                        variables,
                                        statements.ToImmutable());
        }

        /// <summary>
        /// bind the program information including statement,function bodies and diagnostics 
        /// </summary>
        /// <param name="globalScope"></param>
        /// <returns></returns>
        public static BoundProgram BindProgram(bool isScript, BoundProgram previous, BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();


            foreach (var function in globalScope.Functions)
            {
                var binder = new Binder(isScript, parentScope, function);
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);

                if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                {
                    binder._diagnostics.ReportAllPathMustReturn(function.Declaration.Identifier.Location);
                }

                functionBodies.Add(function, loweredBody);
                diagnostics.AddRange(binder.Diagnostics);
            }


            if (globalScope.MainFunction != null && globalScope.Statement.Any())
            {
                var body = Lowerer.Lower(new BoundBlockStatement(globalScope.Statement));
                functionBodies.Add(globalScope.MainFunction, body);
            }

            else if (globalScope.ScriptFunction!=null)
            {
                var statements = globalScope.Statement;

                if (statements.Length == 1 &&
                    statements[0] is BoundExpressionStatement es &&
                    es.Expression.Type != TypeSymbol.Void) 
                {
                    statements = statements.SetItem(0, new BoundReturnStatement(es.Expression));
                }
                else if(statements.Any()&&statements.Last().Kind!= BoundNodeKind.ReturnStatement)
                {
                    var nullValue = new BoundLiteralExpression("");
                    statements = statements.Add(new BoundReturnStatement(nullValue));
                }

                var body = Lowerer.Lower(new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.ScriptFunction, body);

            }




            return new BoundProgram(previous,
                                    diagnostics.ToImmutable(),
                                    globalScope.MainFunction,
                                    globalScope.ScriptFunction,
                                    functionBodies.ToImmutable());
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
                    _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            // for function return type
            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;


            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
            if (function.Declaration.Identifier.Text != null &&
                !_scope.TryDeclareFunction(function))
            {
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
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

                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }

            return result;
        }

        public DiagnosticBag Diagnostics => _diagnostics;
        private BoundStatement BindGlobalStatement(StatementSyntax syntax)
        {
            return BindStatement(syntax, isGlobal: true);
        }


        private BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
        {
            var result = BindStatementInternal(syntax);
            if (!_isScript || !isGlobal)
            {
                if (result is BoundExpressionStatement es)
                {
                    var isAllowedExpression = es.Expression.Kind == BoundNodeKind.CallExpression ||
                                              es.Expression.Kind == BoundNodeKind.AssignmentExpression ||
                                              es.Expression.Kind == BoundNodeKind.ErrorExpression;
                    if (!isAllowedExpression)
                    {
                        _diagnostics.ReportInvalidExpressionStatement(syntax.Location);
                    }
                }
            }
            return result;
        }



        private BoundStatement BindStatementInternal(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatmentSyntax)syntax);
                case SyntaxKind.BreakStatement:
                    return BindBreakStatement((BreakStatementSyntax)syntax);
                case SyntaxKind.ContinueStatement:
                    return BindContinueStatement((ContinueStatementSyntax)syntax);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatmentSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindReturnStatement(ReturnStatmentSyntax syntax)
        {
            var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

            if (_function == null)
            {
                if (_isScript)
                {
                    // Ignore   allow both return with and without values
                    if (expression ==null)
                    {
                        expression = new BoundLiteralExpression("");
                    }
                }
 
            }
            else
            {
                if (_function.Type == TypeSymbol.Void)
                {
                    if (expression != null)
                    {
                        _diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, _function.Name);
                    }

                }
                else
                {
                    if (expression == null)
                    {
                        _diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Type);
                    }
                    else
                    {
                        expression = BindConversion(syntax.Expression.Location, expression, _function.Type);
                    }


                }
            }



            return new BoundReturnStatement(expression);
        }

        private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BoundErrorStatement();
            }
            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BoundErrorStatement();
            }
            var breaklable = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breaklable);
        }

        private BoundStatement BoundErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
        }

        private BoundStatement BindForStatement(ForStatmentSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var identifier = syntax.Identifier;
            var variable = BindVariableDeclaration(identifier, true, TypeSymbol.Int);


            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }


        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLable);
            return new BoundWhileStatement(condition, body, breakLabel, continueLable);
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

            var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, variableType);

            //check if the target can be converted from the initializer's type 
            var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);



            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax == null)
            {
                return null;
            }

            var type = LookupType(syntax.Identifier.Text);
            if (type == null)
            {
                _diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
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
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Location);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            {
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);
            }


            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();
            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }



            var symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);
            if (symbol == null)
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            var function = symbol as FunctionSymbol;
            if (function == null)
            {
                _diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }


            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                //_diagnostics.ReportWrongArguementCount(syntax.Span,function.Name,function.Parameters.Length,syntax.Arguments.Count);

                TextSpan span;
                if (syntax.Arguments.Count > function.Parameters.Length)
                {
                    SyntaxNode firstExceedingNode;
                    if (function.Parameters.Length > 0)
                    {
                        firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                    }
                    else
                    {
                        firstExceedingNode = syntax.Arguments[0];
                    }
                    var lastExceedingArgument = syntax.Arguments.Last();
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }
                var location = new TextLocation(syntax.SyntaxTree.Text, span);
                _diagnostics.ReportWrongArguementCount(location, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }


            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                var argumentLocation = syntax.Arguments[i].Location;
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];
                boundArguments[i] = BindConversion(argumentLocation, argument, parameter.Type);
            }


            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }


        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {

            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported an error so we can just return an error expression
                return new BoundErrorExpression();
            }

            //if (!_scope.TryLookupVariable(name, out var variable))
            //{
            //_diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            var variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
            {
                return new BoundErrorExpression();
            }
            //  }


            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var boundExpresion = BindExpression(syntax.Expression);



            //_diagnostics.ReportUndefinedName(syntax.Identifier.Span, name);
            var variable = BindVariableReference(syntax.Identifier);
            if (variable == null)
                return new BoundErrorExpression();


            if (variable.IsReadOnly)
            {
                _diagnostics.ReportCannotAssign(syntax.EqualToken.Location, name);
            }

            var convertedExpression = BindConversion(syntax.Expression.Location, boundExpresion, variable.Type);

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
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
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
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                    boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }


        private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;

            VariableSymbol variable = _function == null
                ? new GlobalVariableSymbol(name, isReadOnly, type)
                : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
            {
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Location, name);
            }

            return variable;
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);

            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextLocation diagnosticLocation, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exist)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
                }
                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);
            }

            if (conversion.IsIdentity)
            {
                return expression;
            }

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariableReference(SyntaxToken identifierToken)
        {
            var name = identifierToken.Text;
            var location = identifierToken.Location;
            switch (_scope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    _diagnostics.ReportUndefinedVariable(location, name);
                    return null;

                default:
                    _diagnostics.ReportNotAVariable(location, name);
                    return null;
            }
        }


        private TypeSymbol LookupType(string name)
        {
            switch (name)
            {
                case "any": return TypeSymbol.Any;
                case "bool": return TypeSymbol.Bool;
                case "int": return TypeSymbol.Int;
                case "string": return TypeSymbol.String;
                default: return null;
            }
        }

        private BoundStatement BindLoopBody(StatementSyntax syntax, out BoundLabel breakLabel, out BoundLabel continueLable)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break_{_labelCounter}");
            continueLable = new BoundLabel($"continue_{_labelCounter}");

            _loopStack.Push((breakLabel, continueLable));
            var body = BindStatement(syntax);
            _loopStack.Pop();

            return body;
        }
    }
}