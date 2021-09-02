using System.Collections.Immutable;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statement { get; }


        public BoundGlobalScope(BoundGlobalScope previous,
            ImmutableArray<Diagnostic> diagnostics,
            FunctionSymbol mainFunction,
            FunctionSymbol scriptFunction,
            ImmutableArray<FunctionSymbol> functions,
            ImmutableArray<VariableSymbol> variable,
            ImmutableArray<BoundStatement> statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
            Variables = variable;
            Statement = statement;
        }
        
        
    }
}