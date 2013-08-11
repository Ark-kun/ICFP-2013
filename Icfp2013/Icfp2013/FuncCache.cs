using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GOLD;

namespace Icfp2013
{
    class FuncCache
    {
        public class CacheEntry
        {
            public string Program;
            public bool HasFold;
            public ulong[] Evals;
            public string[] UsedOps;

            public FunctionTreeNode Func;
            public int FuncSize;

            public void ParseProgram()
            {
                var parser = FuncCache.S.funcParser;
                parser.Restart();
                parser.Open(ref Program);
                ParseMessage msg;
                do
                {
                    msg = parser.Parse();
                } while (msg == ParseMessage.TokenRead || msg == ParseMessage.Reduction);

                if (msg != ParseMessage.Accept)
                {      
                     throw new Exception("Error in parser");                    
                }

                EvaluationContext ctx = new EvaluationContext();
                Func = ParseNode((Reduction)parser.CurrentReduction, ctx);
                UsedOps = Func.GetUsedOps().ToArray();
                FuncSize = Func.GetSize();
            }

//Identifier = {Letter}+
//UnaryOperator = 'shl1' | 'shr1' | 'shr4' | 'shr16' | 'not'
//BinaryOperator = 'and' | 'or' | 'xor' | 'plus' 
//TernaryOperator = 'if0'
//FoldOperator = 'fold' 
//"Start Symbol" = <Lambda>
//<Lambda> ::= '(lambda (' <IdentifiersList> ')' <Function> ')'
//<IdentifiersList> ::= Identifier | <IdentifiersList> Identifier
//<Function> ::= <NullaryFunc> | <UnaryFunc> | <BinaryFunc> | <TernaryFunc> | <FoldFunc>
//<NullaryFunc> ::= Identifier | '0' | '1'
//<UnaryFunc> ::= '(' UnaryOperator <Function> ')' 
//<BinaryFunc> ::= '(' BinaryOperator <Function> <Function> ')'
//<TernaryFunc> ::= '(' TernaryOperator <Function> <Function> <Function> ')'
//<FoldFunc> ::= '(' FoldOperator <Function> <Function> <Lambda> ')'

            FunctionTreeNode ParseNode(Reduction reduction, EvaluationContext ctx)
            {
                string name = reduction.Parent.Head().Name();
                if (name == "Lambda")
                {
                    return ParseNode((Reduction)reduction[3].Data, ctx);
                }
             
                Operators.Op op;
                FunctionTreeNode func;
                switch (reduction.Parent.Head().Name())
                {
                    case "Lambda":
                        return ParseNode((Reduction)reduction[3].Data, ctx);
                    case "NullaryFunc":
                        op = Searcher.Ops.SelectMany(a => a).First(a => a.ToString().ToLowerInvariant() == (string)reduction[0].Data);
                        return new FunctionTreeNode(ctx) { Operator = op };
                    case "UnaryFunc":
                        op = Searcher.Ops.SelectMany(a => a).First(a => a.ToString().ToLowerInvariant() == (string)reduction[1].Data);
                        func = new FunctionTreeNode(ctx) { Operator = op };
                        func.Children.Add(ParseNode((Reduction)reduction[2].Data, ctx));
                        return func;
                    case "BinaryFunc":
                        op = Searcher.Ops.SelectMany(a => a).First(a => a.ToString().ToLowerInvariant() == (string)reduction[1].Data);
                        func = new FunctionTreeNode(ctx) { Operator = op };
                        func.Children.Add(ParseNode((Reduction)reduction[2].Data, ctx));
                        func.Children.Add(ParseNode((Reduction)reduction[3].Data, ctx));
                        return func;
                    case "TernaryFunc":
                    case "FoldFunc":
                        op = Searcher.Ops.SelectMany(a => a).First(a => a.ToString().ToLowerInvariant() == (string)reduction[1].Data);
                        func = new FunctionTreeNode(ctx) { Operator = op };
                        func.Children.Add(ParseNode((Reduction)reduction[2].Data, ctx));
                        func.Children.Add(ParseNode((Reduction)reduction[3].Data, ctx));
                        func.Children.Add(ParseNode((Reduction)reduction[4].Data, ctx));
                        return func;
                }

                return null;
            }

            public override string ToString()
            {
                return Program;
            }
        }

        public static FuncCache S { get; private set; }
        public List<CacheEntry> Entries = new List<CacheEntry>();
        public List<CacheEntry> FilteredEntries;
        GOLD.Parser funcParser = new GOLD.Parser();

        public static int CacheSize = -1;

        public FuncCache()
        {
            S = this;          
        }

        public void ReadCache(Problem p)
        {
            if (CacheSize == Searcher.UseCacheOfSize) return;

            CacheSize = Searcher.UseCacheOfSize;
            if (CacheSize == 0) return;

            funcParser.LoadTables("bv.egt");
            funcParser.TrimReductions = true;
            ////debug
            //var ce = new CacheEntry() { Program = "(lambda (x) (fold (shl1 x) 1 (lambda (y z) (shl1 y))))" };
            //ce.ParseProgram();

            string[] allLines = File.ReadAllLines("cache" + (CacheSize < 3 ? 3 : CacheSize) + ".txt");

            for (int i = 0; i < allLines.Length; i++)
            {
                string[] split = allLines[i].Split('\t');
                var cacheEntry = new CacheEntry()
                {
                    Evals = split[2].Split(',').Select(a => ulong.Parse(a, System.Globalization.NumberStyles.HexNumber)).ToArray(),
                    HasFold = bool.Parse(split[1]),
                    Program = split[0]
                };
                cacheEntry.ParseProgram();
                Entries.Add(cacheEntry);
            }

            FilterByAllowedOperatorsAndSize(p.AllowedOperatorsStrings);
        }

        public void FilterByAllowedOperatorsAndSize(string[] allowedOps)
        {
            FilteredEntries = Entries.Where(e => e.FuncSize == CacheSize + 1 && e.UsedOps.All(op => allowedOps.Contains(op))).ToList();

            CacheEntry test = new CacheEntry() { Program = "(lambda (x) (plus (or x (not 1)) 1))" };
            test.ParseProgram();

            Evals tev = new Evals()
            {
                Values = Enumerable.Range(0, 1024).Select(i =>
                    {
                        test.Func.Context.Arg = Program.EvalArgs[i];
                        return test.Func.Eval();
                    }).ToArray()
            };


            foreach (var antry in FilteredEntries)
            {
                Evals aev = new Evals() { Values = antry.Evals };
                if (aev.Equals(tev))
                {
                }
            }

        }
    }
}
