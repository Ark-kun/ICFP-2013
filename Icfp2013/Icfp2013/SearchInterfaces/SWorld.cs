using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dzugaru.Search;
using System.Collections;

namespace Icfp2013
{
    class SWorld : IWorld
    {
        //debug
        static int NORGFreq = 10000;
        int NORGCounter;
        public int NumOfRightGuesses;

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
            //FunctionTreeNode func = ((TreeOfTreesNode)state).FunctionTreeRoot;
            //int heuristic = 0;

            //for (int i = 0; i < 64; i++)
            //{
            //    func.Context.Arg = Problem.Evals[i][0];
            //    ulong funcEval = func.Eval();
            //    ulong targetEval = Problem.Evals[i][1];

            //    if (funcEval != targetEval)
            //    {
            //        heuristic++;
            //    }

            //    //for (int j = 0; j < 64; j++)
            //    //{
            //    //    if ((funcEval >> j & 1) != (targetEval >> j & 1))
            //    //    {
            //    //        heuristic++;
            //    //    }
            //    //}
            //}

            //NORGCounter++;
            //NumOfRightGuesses += heuristic;
            //if (NORGCounter % NORGFreq == 0)
            //{
            //    Console.WriteLine("Average heuristic: " + NumOfRightGuesses / (double)NORGFreq);
            //    NumOfRightGuesses = 0;
            //}

            //return heuristic;

            return 0;
        }

        public double GetTransitionCost(IState from, IAction action)
        {
            return ((SAction)action).Cost;
        }
    }
}
