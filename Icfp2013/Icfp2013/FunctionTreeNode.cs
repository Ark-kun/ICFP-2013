using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class FunctionTreeNode : IEquatable<FunctionTreeNode>
    {        
        public EvaluationContext Context;

        public FunctionTreeNode Parent;
        public List<FunctionTreeNode> Children;

        public Operators.Op Operator;
        public FunctionTreeNode LastClonedTo;
        public bool IsInsideFoldLambda;

        public FunctionTreeNode(EvaluationContext ctx, FunctionTreeNode parent = null)
        {
            Context = ctx;
            Parent = parent;
            Children = new List<FunctionTreeNode>();
        }

        public ulong Eval()
        {            
            Operator.Context = Context;
            var foldOp = Operator as Operators.Fold;
            if (foldOp != null)
            {
                return foldOp.FoldEval(Children[0].Eval(), Children[1].Eval(), Children[2]);
            }
            else
            {
                return Operator.Eval(Children.Select(a => a.Eval()).ToArray());
            }
        }

        public int GetSize()
        {
            //TODO: exceptions like fold?
            return (Operator is Operators.Fold ? 2 : 1) + Children.Sum(c => c.GetSize());
        }

        public FunctionTreeNode Clone()
        {
            var clone = new FunctionTreeNode(Context, Parent) { Operator = this.Operator, IsInsideFoldLambda = this.IsInsideFoldLambda };
            this.LastClonedTo = clone;
            clone.Children = this.Children.Select(ch => ch.Clone()).ToList();
            return clone;
        }

        public override string ToString()
        {
            return new TreeVisualizer().Visualize(this);
        }

        public bool Equals(FunctionTreeNode other)
        {
            if (Children.Count != other.Children.Count) return false;

            for (int i = 0; i < Children.Count; i++)
			{
			    if(!Children[i].Equals(other.Children[i])) return false;
			}

            return this.Operator == other.Operator; 
        }

        public bool IsTFold()
        {
            return this.Operator is Operators.Fold && this.Children[0].Operator is Operators.Arg && this.Children[1].Operator is Operators.Zero;
        }
    }
}
