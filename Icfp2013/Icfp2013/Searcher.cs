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

        public static Operators.Op[][] Ops = new Op[][] 
        {
            new Op[]{
            //0
            new Zero(),
            new One(),
            new Arg(0),
            new Arg(1),
            new Arg(2)
            },

            new Op[]{
            //1
            new Not(),
            new Shl1(),
            new Shr1(),
            new Shr4(),
            new Shr16()
            },

            new Op[]{
            //2
            new And(),
            new Or(),
            new Plus(),
            new Xor(),
            },

            new Op[]{
            //3
            new If0(),
            new Fold()
            }
        };

        public Searcher()
        {
            //RootNode = CurrentNode = new FunctionTreeNode(Context);                     
        }

        public FunctionTreeNode Find(Problem p)
        {
            SWorld world = new SWorld();
            var result = Dzugaru.Search.Solver.IterativeDeepeningTreeSearch(world, p);

            FunctionTreeNode res = ((SAction)result.Last()).Next.FunctionTreeRoot;
            

            return res;
        }

        //public void Find(Problem p)
        //{
        //    this.problem = p;
        //    Context = new EvaluationContext();
        //    CurrentNode = new TreeOfTreesNode(new FunctionTreeNode(Context), 1);

        //    for (; ; )
        //    {
        //        if (!Iterate()) break;
        //    }
        //}

        //bool Iterate()
        //{
        //    if (CurrentNode.Size == problem.Size)
        //    {
        //        if (CheckProblem())
        //        {
        //            Console.WriteLine("Solution found! " + CurrentNode.FunctionTreeRoot.ToString());
        //            return false;
        //        }                
        //    }

        //    if (CurrentNode.Children == null && CurrentNode.Size < problem.Size)
        //    {
        //        CurrentNode.CreateChildren();
        //        CurrentNode = CurrentNode.Children[0];
        //    }
        //    else if (CurrentNode.Parent == null) return false;
        //    else
        //    {
        //        int childIndex = CurrentNode.Parent.Children.IndexOf(CurrentNode);
        //        if (childIndex == CurrentNode.Parent.Children.Count - 1)
        //        {
        //            CurrentNode = CurrentNode.Parent;
        //        }
        //        else
        //        {
        //            CurrentNode.Children = null; //free memory
        //            CurrentNode = CurrentNode.Parent.Children[childIndex + 1];
        //        }
        //    }

        //    return true;
        //}

        //bool CheckProblem()
        //{
            
        //}
    }
}
