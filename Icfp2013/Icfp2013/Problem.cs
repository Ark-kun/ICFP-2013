using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dzugaru.Search;

namespace Icfp2013
{
    class Problem : IProblem
    {
        //debug
        static int NORGFreq = 1000;
        int NORGCounter;
        public int NumOfRightGuesses;

        EvaluationContext ctx;

        public string ID;
        public int Size;
        public ulong[][] Evals;
        public string Solution;
        public HashSet<Tuple<int, int>> AllowedOperators;
        public string[] AllowedOperatorsStrings;

        public bool IsTFoldProblem;
        public bool IsCacheGenerator;

        

        public override string ToString()
        {            
            return "[" + string.Join(",", AllowedOperatorsStrings) + "] " + Solution;
        }

        public Problem()
        {
            ctx = new EvaluationContext();
        }

        public IState InitialState
        {
            get
            {
                var func = new FunctionTreeNode(ctx) { Operator = Searcher.Ops[0][0] };
                func.CalcFuncEvals(this);
                return new TreeOfTreesNode(func, 1);
            }
        }

        public bool IsGoalState(IState state)
        {
            if (IsCacheGenerator) return false;

            var func = ((TreeOfTreesNode)state).FunctionTreeRoot;
            //if (func.ToString().Contains("plus (if0 0 x 1) x"))
            //{
            //    System.IO.File.AppendAllText("allguesses.txt", func + "\r\n");
            //}

            //for (int i = 0; i < Evals.Length; i++)
            //{                
            //    if (((TreeOfTreesNode)state).FunctionTreeRoot.CalculatedEvals.Values[i] != Evals[i][1])
            //    {                   
            //        return false;
            //    }
            //}


            if (((TreeOfTreesNode)state).FunctionTreeRoot.IsEvalsRight)
            {
                if (((TreeOfTreesNode)state).FunctionTreeRoot.HasCacheOp)
                {
                    Console.WriteLine("Solved using cache!!!");
                }
                return true;
            }
            else
            {
                return false;
            }

            //return true;
        }

        public void SetAllowedOperators(string[] ops)
        {
            AllowedOperatorsStrings = ops;
            AllowedOperators = new HashSet<Tuple<int, int>>();
            for (int a = 1; a <=3; a++)
            {
                for (int j = 0; j < Searcher.Ops[a].Length; j++)
                {
                    string sopString = Searcher.Ops[a][j].ToString().ToLowerInvariant();
                    if (ops.Contains(sopString))
                    {
                        AllowedOperators.Add(new Tuple<int, int>(a, j));
                    }
                }
            }

            if (!AllowedOperators.Any(a => a.Item1 == 1))
            {
                AllowedOperators.Add(new Tuple<int, int>(1, 0));
            }

            if (!AllowedOperators.Any(a => a.Item1 == 2))
            {
                AllowedOperators.Add(new Tuple<int, int>(2, 0));
            }

            IsTFoldProblem = ops.Contains("tfold");            
        }
    }
}
