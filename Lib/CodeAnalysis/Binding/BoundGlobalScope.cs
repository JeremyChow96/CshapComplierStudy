using System.Collections.Immutable;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variable { get; }
        public BoundStatement Statement { get; }

        public BoundGlobalScope(BoundGlobalScope previous,
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<VariableSymbol> variable,
            BoundStatement statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variable = variable;
            Statement = statement;
        }
        
        
    }
}