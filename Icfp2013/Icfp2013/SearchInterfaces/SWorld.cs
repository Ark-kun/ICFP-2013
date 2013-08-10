using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dzugaru.Search;

namespace Icfp2013
{
    class SWorld : IWorld
    {
        public Problem Problem;

        public IState GetActionResult(IState state, IAction action)
        {
            return ((SAction)action).Next;
        }

        public IEnumerable<IAction> GetActionsInState(IState state)
        {
            TreeOfTreesNode node = (TreeOfTreesNode)state;
            return node.GetNext(node.FunctionTreeRoot, Problem);
        }

        public double GetHeuristic(IState state, IProblem problem)
        {
            return 0;
        }

        public double GetTransitionCost(IState from, IAction action)
        {
            return 0;
        }
    }
}
