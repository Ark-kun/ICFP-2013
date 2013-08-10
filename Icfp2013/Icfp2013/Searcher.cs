using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icfp2013.Operators;

namespace Icfp2013
{
    class Searcher
    {
        //debug
        public int VariantsCount;

        Problem problem;

        EvaluationContext Context;
        TreeOfTreesNode CurrentNode; 

        public static Func<Operators.Op>[][] Ops = new Func<Op>[][] 
        {
            new Func<Op>[]{
            //0
            () => new Zero(),
            () => new One(),
            () => new Arg(),
            },

            new Func<Op>[]{
            //1
            () => new Not(),
            () => new Shl1(),
            () => new Shr1(),
            () => new Shr4(),
            () => new Shr16()
            },

            new Func<Op>[]{
            //2
            () => new And(),
            () => new Or(),
            () => new Plus(),
            () => new Xor(),
            },

            new Func<Op>[]{
            //3
            () => new If0()
            }
        };

        public Searcher()
        {
           
            

            //RootNode = CurrentNode = new FunctionTreeNode(Context);                     
        }

        public void Find(Problem p)
        {
            this.problem = p;
            Context = new EvaluationContext();
            CurrentNode = new TreeOfTreesNode(new FunctionTreeNode(Context), 1);

            for (; ; )
            {
                if (!Iterate()) break;
            }
        }

        bool Iterate()
        {
            if (CurrentNode.Size == problem.Size)
            {
                if (CheckProblem())
                {
                    Console.WriteLine("Solution found! " + CurrentNode.FunctionTreeRoot.ToString());
                    return false;
                }                
            }

            if (CurrentNode.Children == null && CurrentNode.Size < problem.Size)
            {
                CurrentNode.CreateChildren();
                CurrentNode = CurrentNode.Children[0];
            }
            else if (CurrentNode.Parent == null) return false;
            else
            {
                int childIndex = CurrentNode.Parent.Children.IndexOf(CurrentNode);
                if (childIndex == CurrentNode.Parent.Children.Count - 1)
                {
                    CurrentNode = CurrentNode.Parent;
                }
                else
                {
                    CurrentNode.Children = null; //free memory
                    CurrentNode = CurrentNode.Parent.Children[childIndex + 1];
                }
            }

            return true;
        }

        bool CheckProblem()
        {
            for (int i = 0; i < problem.Evals.Length; i++)
            {
                Context.Arg = problem.Evals[i][0];
                if (CurrentNode.FunctionTreeRoot.Eval() != problem.Evals[i][1]) return false;
            }

            return true;
        }
    }
}
