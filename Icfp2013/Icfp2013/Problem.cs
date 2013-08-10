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
        EvaluationContext ctx;

        public string ID;
        public int Size;
        public ulong[][] Evals;
        public string Solution;
        public HashSet<Tuple<int, int>> AllowedOperators;

        public override string ToString()
        {            
            return Solution;
        }

        public Problem()
        {
            ctx = new EvaluationContext();
        }

        public IState InitialState
        {
            get
            {
                return new TreeOfTreesNode(new FunctionTreeNode(ctx) { Operator = Searcher.Ops[0][0] }, 1);
            }
        }

        public bool IsGoalState(IState state)
        {
            for (int i = 0; i < Evals.Length; i++)
            {
                ctx.Arg = Evals[i][0];
                if (((TreeOfTreesNode)state).FunctionTreeRoot.Eval() != Evals[i][1]) return false;
            }

            return true;
        }

        public void SetAllowedOperators(string[] ops)
        {
            AllowedOperators = new HashSet<Tuple<int, int>>();
            for (int a = 1; a <=3; a++)
            {
                for (int j = 0; j < Searcher.Ops[a].Length; j++)
                {
                    string sopString = Searcher.Ops[a][j].ToString().ToLowerInvariant();
                    if (ops.Contains(sopString) || ops.Contains("tfold") && sopString == "fold")
                    {
                        AllowedOperators.Add(new Tuple<int, int>(a, j));
                    }
                }
            }
        }
    }
}
