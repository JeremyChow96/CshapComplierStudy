using System.Collections.Immutable;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundGlobalScope GlobalScope { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies { get; }
        public DiagnosticBag Diagnostics { get; }

        public BoundProgram(BoundGlobalScope globalScope, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies, DiagnosticBag diagnostics)
        {
            GlobalScope = globalScope;
            FunctionBodies = functionBodies;
            Diagnostics = diagnostics;
        }
    }
}