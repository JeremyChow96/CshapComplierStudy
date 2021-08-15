﻿namespace Lib.CodeAnalysis.Symbols
{
    public  sealed class  ParameterSymbol :LocalVariableSymbol
    {
       

        internal ParameterSymbol(string name,TypeSymbol type) : base(name,isReadOnly:true,type)
        {
         
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}