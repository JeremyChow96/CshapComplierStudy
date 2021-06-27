using System.Collections.Immutable;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variable { get; }
        public BoundExpression Expression { get; }

        public BoundGlobalScope(BoundGlobalScope previous,
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<VariableSymbol> variable,
            BoundExpression expression)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variable = variable;
            Expression = expression;
        }
        
        
    }
}