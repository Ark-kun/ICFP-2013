using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class FunctionTreeNode
    {        
        public EvaluationContext Context;

        public FunctionTreeNode Parent;
        public List<FunctionTreeNode> Children;

        public Operators.Op Operator;
        public FunctionTreeNode LastClonedTo;

        public int IterationCurrentOp;

        public FunctionTreeNode(EvaluationContext ctx, FunctionTreeNode parent = null)
        {
            Context = ctx;
            Parent = parent;
            Children = new List<FunctionTreeNode>();
        }

        public ulong Eval()
        {
            if (Operator is Operators.Arg)
            {
                return Context.Arg;
            }
            else
            {
                return Operator.Eval(Children.Select(a => a.Eval()).ToArray());
            }
        }

        public int GetSize()
        {
            //TODO: exceptions like fold?
            return 1 + Children.Sum(c => c.GetSize());
        }

        public FunctionTreeNode Clone()
        {
            var clone = new FunctionTreeNode(Context, Parent) { Operator = this.Operator };
            this.LastClonedTo = clone;
            clone.Children = this.Children.Select(ch => ch.Clone()).ToList();
            return clone;
        }

        public override string ToString()
        {
            return new TreeVisualizer().Visualize(this);
        }
    }
}
