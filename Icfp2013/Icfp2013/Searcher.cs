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

        public static HashSet<Evals> AllEvals;

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
           

            AllEvals = new HashSet<Evals>();
            SWorld world = new SWorld() { Problem = p };
            var result = Dzugaru.Search.Solver.IterativeDeepeningTreeSearch(world, p);
            //var result = Dzugaru.Search.Solver.SimplifiedMemoryBoundAStar(world, p, 1000);

            FunctionTreeNode res = ((SAction)result.Last()).Next.FunctionTreeRoot;
            

            return res;
        }         
    }
}
