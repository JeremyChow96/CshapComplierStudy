using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using complier.CodeAnalysis;
using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Binding;
using Lib.CodeAnalysis.Symbols;

namespace Lib.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            var name = $"Label_{++_labelCount}";
            return new BoundLabel(name);
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {

            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);
            while (stack.Count>0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                    {
                        stack.Push(s);
                    }
                }
                else
                {
                    builder.Add(current);
                }
             
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }
        
        
        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            // if <condition>
            //      <then>
            //
            //  ---->
            // gotoFalse <condition> end
            //  <then>
            //  end:
            //
            //=================================


            if (node.ElseStatement == null)
            {
                var endLabel = GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var result =
                    new BoundBlockStatement(
                        ImmutableArray.Create<BoundStatement>(gotoFalse, node.ThenStatement, endLabelStatement));
                return RewriteStatement(result);
            }
            else
            {
                // if <condition>
                //    <then>
                // else
                //    <else>
                //
                //  ---->
                // gotoFalse <condition> else
                //           <then>
                // goto end
                // else :
                //     <else>
                // end :


                var elseLabel = GenerateLabel();
                var endLabel = GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
                var gotoEndStatement = new BoundGotoStatement(endLabel);
                var elseLabelStatement = new BoundLabelStatement(elseLabel);
                var endLabelStatement = new BoundLabelStatement(endLabel);

                var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoFalse,
                    node.ThenStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement));
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // while <condition>
            //       <body>
            //  
            //   -----> 
            //   goto check
            //   continue :
            //      <body>
            //   check :
            //     gotoTrue <condition> continue
            //  break

            var checkLabel = GenerateLabel();
            var gotoCheck = new BoundGotoStatement(checkLabel);
            var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var gotoTrue = new BoundConditionalGotoStatement(node.ContinueLabel, node.Condition);
            var breakLabel = new BoundLabelStatement(node.BreakLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue,
                breakLabel));
            return RewriteStatement(result);
        }


        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            //--->
            //
            //{
            //    var <var> = <lower>
            //    while(<var> <= <upper>)
            //     {
            //         <body>
            //         continue:
            //         <var> = <var> + 1
            //     }
            //     break
            // }

            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            var variableExpression = new BoundVariableExpression(node.Variable);
            var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
            var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
            
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOpertor.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
                new BoundVariableExpression(upperBoundSymbol));
      

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOpertor.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                        new BoundLiteralExpression(1)
                    )
                )
            );
            var continueLabel = new BoundLabelStatement(node.ContinueLabel);
            var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, continueLabel, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody,node.BreakLabel,GenerateLabel());
            var result =
                new BoundBlockStatement(ImmutableArray.Create<BoundStatement>
                    (variableDeclaration,
                    upperBoundDeclaration,
                    whileStatement));

            return RewriteStatement(result);
        }
    }
}