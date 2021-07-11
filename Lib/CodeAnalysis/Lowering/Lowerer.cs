﻿using System.Collections.Immutable;
using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Binding;

namespace Lib.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        {
        }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
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
            //         <var> = <var> + 1
            //     }
            // }

            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            var variableExpression = new BoundVariableExpression(node.Variable);
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOpertor.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
                node.UpperBound);


            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOpertor.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                        new BoundLiteralExpression(1)
                        )
                    )
                );

            var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody);
            var result =
                new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));

            return RewriteStatement(result);
        }

        // protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        // {
        // }
    }
}