using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
    }


    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
        where T : SyntaxNode
    {
        // this is how  Roslyn deal with parameters. a genius way!  
        private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            _nodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (_nodesAndSeparators.Length + 1) / 2;

        public T this[int index] => (T) _nodesAndSeparators[index * 2];

        public SyntaxToken GetSeparator(int index)
        {
            if (index == Count - 1)
            {
                return null;
            }

            return (SyntaxToken) _nodesAndSeparators[index * 2 + 1];
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public override ImmutableArray<SyntaxNode> GetWithSeparators() => _nodesAndSeparators;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}