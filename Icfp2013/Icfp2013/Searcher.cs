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
        int depthLimit;

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

        public Searcher(int depthLimit)
        {
            this.depthLimit = depthLimit;
            Context = new EvaluationContext();
            CurrentNode = new TreeOfTreesNode(new FunctionTreeNode(Context), 1);
            

            //RootNode = CurrentNode = new FunctionTreeNode(Context);                     
        }

        public void Find()
        {
            for (; ; )
            {
                if (!Iterate()) break;
            }
        }

        bool Iterate()
        {
            if (CurrentNode.Size == depthLimit)
            {
                Console.WriteLine(CurrentNode.FunctionTreeRoot);
                //Search all operator combinations
            }

            if (CurrentNode.Children == null && CurrentNode.Size < depthLimit)
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

        void FindOps(FunctionTreeNode root)
        {
            FunctionTreeNode currentNode = root;

           
        }
    }
}
