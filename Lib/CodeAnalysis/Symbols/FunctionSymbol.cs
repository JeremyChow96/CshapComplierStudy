using System.Collections.Immutable;

namespace Lib.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameter, TypeSymbol type) : base(name)
        {
            Parameter = parameter;
            Type = type;
        }

        public ImmutableArray<ParameterSymbol> Parameter { get; }
        public TypeSymbol Type { get; }


        public override SymbolKind Kind => SymbolKind.Function;
    }
}