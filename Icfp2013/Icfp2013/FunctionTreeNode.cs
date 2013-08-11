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

        public Evals CalculatedEvals;
        public bool IsEvalsRight;
        public bool HasCacheOp;

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
                ulong[] args = new ulong[Children.Count];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = Children[i].Eval();
                }
                return Operator.Eval(args);
            }
        }

        public int GetSize()
        {
            int size = 0;
            if(Operator is Operators.CachedOp)
            {
                size = ((Operators.CachedOp)Operator).CacheEntry.FuncSize;
            }
            else
            {
                size = 1;
            }

            size += Children.Sum(c => c.GetSize());
            return size;
        }

        public FunctionTreeNode Clone()
        {
            var clone = new FunctionTreeNode(Context, Parent) { Operator = this.Operator, IsInsideFoldLambda = this.IsInsideFoldLambda, HasCacheOp = this.HasCacheOp };
            this.LastClonedTo = clone;

            clone.Children = new List<FunctionTreeNode>(Children.Count);
            for (int i = 0; i < Children.Count; i++)
            {
                clone.Children.Add(Children[i].Clone());
            }
            
            return clone;
        }

        public override string ToString()
        {
            return new TreeVisualizer().Visualize(this, false);
        }
        
        public override bool Equals(object obj)
        {
            return Equals((FunctionTreeNode)obj);
        }

        public override int GetHashCode()
        {
            return CalculatedEvals.GetHashCode();
        }

        public bool Equals(FunctionTreeNode other)
        {
            return false;
            //return this.CalculatedEvals.Equals(other.CalculatedEvals); 
        }       
        

        public void CalcFuncEvals(Problem p)
        {
            //EvalsCount++;
            //int amount = 256;
            //Evals ev = new Evals() { Values = new ulong[amount] };
            //for (int i = 0; i < amount; i++)
            //{
            //    Context.ArgIndex = i;
            //    Context.Arg = Program.EvalArgs[i];
            //    ev.Values[i] = 0;// Eval();
            //}
            //CalculatedEvals = ev;

            if (p.IsTFoldProblem)
            {
                var tfoldRoot = new FunctionTreeNode(Context) { Operator = new Operators.Fold() };
                tfoldRoot.Children.Add(new FunctionTreeNode(Context) { Operator = new Operators.Arg(0) });
                tfoldRoot.Children.Add(new FunctionTreeNode(Context) { Operator = new Operators.Zero() });
                tfoldRoot.Children.Add(this);

                int amount = 512;
                for (int i = 0; i < amount; i++)
                {
                    Context.ArgIndex = i;
                    Context.Arg = Program.EvalArgs[i];
                    if (tfoldRoot.Eval() != p.Evals[i][1])
                    {
                        IsEvalsRight = false;
                        return;
                    }
                }

                IsEvalsRight = true;
            }
            else
            {
                int amount = 512;
                for (int i = 0; i < amount; i++)
                {
                    Context.ArgIndex = i;
                    Context.Arg = Program.EvalArgs[i];
                    if (Eval() != p.Evals[i][1])
                    {
                        IsEvalsRight = false;
                        return;
                    }
                }

                IsEvalsRight = true;
            }

            
        }

        public bool IsTFold()
        {
            return this.Operator is Operators.Fold && this.Children[0].Operator is Operators.Arg && this.Children[1].Operator is Operators.Zero;
        }

        public IEnumerable<string> GetUsedOps()
        {
            return GetAllUsedOps().ToArray().Distinct();
        }

        IEnumerable<string> GetAllUsedOps()
        {
            if (Operator.Arity > 0)
            {
                yield return Operator.ToString().ToLowerInvariant();
            }
            foreach (var ch in Children)
            {
                foreach (var op in ch.GetUsedOps())
                {
                    yield return op;
                }
            }
        }
    }
}
