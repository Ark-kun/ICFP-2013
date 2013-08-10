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
        public List<Tuple<int, int>> AllowedOperators;

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
    }
}
