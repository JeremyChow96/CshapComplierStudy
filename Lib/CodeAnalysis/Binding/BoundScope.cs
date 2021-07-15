﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        public BoundScope Parent { get; }
        private Dictionary<string, VariableSymbol> _variables = new Dictionary<string, VariableSymbol>();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }


        public bool TryDeclare(VariableSymbol variable)
        {
            if (_variables.ContainsKey(variable.Name))
            {
                return false;
            }

            _variables.Add(variable.Name, variable);
            return true;
        }
        public bool TryLookup(string name, out VariableSymbol variable)
        {
            if (_variables.TryGetValue(name,out variable))
            {
                return true;
            }

            if (Parent ==null)
            {
                return false;
            }

            return Parent.TryLookup(name, out variable);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return _variables.Values.ToImmutableArray();
        }
        
        
    }
}