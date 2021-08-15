using System.Collections.Immutable;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies { get; }
        public BoundBlockStatement Statement { get; }

        public BoundProgram(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<FunctionSymbol,BoundBlockStatement> functionBodies, BoundBlockStatement statement)
        {
            Diagnostics = diagnostics;
            FunctionBodies = functionBodies;
            Statement = statement;
        }
    }
}