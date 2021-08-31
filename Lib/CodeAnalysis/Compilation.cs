using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Lib.CodeAnalysis.Symbols;
using System;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace complier.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope _globalScope;

        //public Compilation(params SyntaxTree[] syntaxTree) :
        //    this(null, syntaxTree)
        //{
   
        //}

        private Compilation(bool isScript, Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(false, null, syntaxTrees);

        }

        public static Compilation CreateScripts(Compilation previous,params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(true, previous, syntaxTrees);
        }

        public bool IsScript { get; }
        public Compilation Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;


        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope,SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();
            while (submission != null)
            {
                var builtinFunctions = typeof(BuiltinFunctions)
                    .GetFields(ReflectionBindingFlags.Public
                               | ReflectionBindingFlags.Static
                               | ReflectionBindingFlags.NonPublic)
                    .Where(f => f.FieldType == typeof(FunctionSymbol))
                    .Select(fun => (FunctionSymbol)fun.GetValue(null))
                    .ToList();

                foreach (var function in submission.Functions)
                {
                    if (seenSymbolNames.Add(function.Name))
                    {
                        yield return function;
                    }
                }
                foreach (var varaible in submission.Variables)
                {
                    if (seenSymbolNames.Add(varaible.Name))
                    {
                        yield return varaible;
                    }
                }

                foreach (var builtin in builtinFunctions)
                {
                    if (seenSymbolNames.Add(builtin.Name))
                    {
                        yield return builtin;
                    }
                }

                submission = submission.Previous;
            }
        }


        //public Compilation ContinueWith(SyntaxTree syntaxTree)
        //{
        //    return new Compilation(this, syntaxTree);
        //}
        
        private BoundProgram GetProgram()
        {
            var preivous = Previous == null ? null : Previous.GetProgram();
            return Binder.BindProgram(IsScript, preivous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            var program = GetProgram();


            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");

            var cfgStatements = !program.Statement.Statements.Any() && program.Functions.Any()
                    ? program.Functions.Last().Value
                    : program.Statement;

            var cfg = ControlFlowGraph.Create(cfgStatements);
            using (var streamWriter  = new StreamWriter(cfgPath))
            {
                cfg.WriteTo(streamWriter);
            }




            if (program.Diagnostics.Any())
            {
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            }
            
            
            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = GetProgram();

            if (program.Statement.Statements.Any())
            {
                program.Statement.WriteTo(writer);
            }
            else
            {
                foreach (var functionBody in program.Functions)
                {
                    if (!GlobalScope.Functions.Contains(functionBody.Key))
                    {
                        continue;
                    }
                    functionBody.Key.WriteTo(writer);
                    writer.WriteLine();
                    functionBody.Value.WriteTo(writer);
                }
            }
       
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var programn = GetProgram();

            //We already check  function'existence before.

            symbol.WriteTo(writer);
            writer.WriteLine();
            if (!programn.Functions.TryGetValue(symbol, out var body))
            {
                return;
            }
            body.WriteTo(writer);
        }
    }
}