using System.Collections.Immutable;
using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Binding;

namespace Lib.CodeAnalysis.Lowering
{
    internal sealed class Lowerer :BoundTreeRewriter
    {
        private Lowerer()
        {
            
        }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }

        // protected override BoundStatement RewriteForStatement(BoundForStatement node)
        // {
        //    
        // }
    }
    
}