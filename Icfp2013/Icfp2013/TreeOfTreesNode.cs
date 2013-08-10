using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dzugaru.Search;

namespace Icfp2013
{
    class TreeOfTreesNode : IState
    {
        static int MaxArity = 3;

        public int Size;
        public FunctionTreeNode FunctionTreeRoot;
        public bool HasFold;

        //public List<TreeOfTreesNode> Children;
        //public TreeOfTreesNode Parent;

        public TreeOfTreesNode(FunctionTreeNode root, int size, TreeOfTreesNode parent = null)
        {
            this.FunctionTreeRoot = root;
            this.Size = size;
            //this.Parent = parent;
        }

        public IEnumerable<IAction> GetNext(FunctionTreeNode node)
        {
            foreach (var ch in node.Children)
            {
                foreach (var a in GetNext(ch))
                {
                    yield return a;
                }
            }

            if (node.Children.Count < MaxArity)
            {
                foreach(var upperOp in GetAllowedOpsUpper(node))
                {
                    foreach(var lowerOp in GetAllowedOpsLower(node, upperOp))
                    {
                        FunctionTreeNode clone = FunctionTreeRoot.Clone();
                        var newNode = new FunctionTreeNode(clone.Context, node.LastClonedTo) { Operator = lowerOp, IsInsideFoldLambda = node.LastClonedTo.IsInsideFoldLambda };
                        node.LastClonedTo.Children.Add(newNode);
                        node.LastClonedTo.Operator = upperOp;

                        var newTreeOfTreesNode = new TreeOfTreesNode(clone, this.Size + 1, this) { HasFold = this.HasFold };
                        if (upperOp is Operators.Fold)
                        {
                            newTreeOfTreesNode.HasFold = true;
                            if(node.LastClonedTo.Children.Count == 3)
                            {
                                newNode.IsInsideFoldLambda = true;
                            }
                        }

                        //Console.WriteLine(newTreeOfTreesNode.FunctionTreeRoot);
                        yield return new SAction() { Next = newTreeOfTreesNode };
                    }
                }
            }
        }

        public IEnumerable<Operators.Op> GetAllowedOpsUpper(FunctionTreeNode node)
        {
            int arity = node.Children.Count + 1;
            for (int i = 0; i < Searcher.Ops[arity].Length; i++)
            {
                if (arity == 3 && HasFold && Searcher.Ops[arity][i] is Operators.Fold) continue; //only one fold allowed per function
                yield return Searcher.Ops[arity][i];
            }
        }

        public IEnumerable<Operators.Op> GetAllowedOpsLower(FunctionTreeNode node, Operators.Op upperOp)
        {           
            for (int i = 0; i < Searcher.Ops[0].Length; i++)
            {
                if (!node.IsInsideFoldLambda && !(upperOp is Operators.Fold) &&
                    (Searcher.Ops[0][i] is Operators.Arg) && ((Operators.Arg)Searcher.Ops[0][i]).ArgIndex != 0) continue; //y and z allowed only inside fold lambda
                yield return Searcher.Ops[0][i];
            }
        }

        //public void CreateChildren()
        //{
        //    Children = new List<TreeOfTreesNode>();
        //    AddChildrenForNode(FunctionTreeRoot);
        //}

        //void AddChildrenForNode(FunctionTreeNode node)
        //{
        //    foreach (var ch in node.Children)
        //    {
        //        AddChildrenForNode(ch);
        //    }

        //    if (node.Children.Count < MaxArity)
        //    {
        //        for (int i = 0; i < Searcher.Ops[0].Length; i++)
        //        {
        //            for (int j = 0; j < Searcher.Ops[node.Children.Count + 1].Length; j++)
        //            {
        //                FunctionTreeNode clone = FunctionTreeRoot.Clone();
        //                node.LastClonedTo.Children.Add(new FunctionTreeNode(clone.Context, node.LastClonedTo) { Operator = Searcher.Ops[0][i] });
        //                node.LastClonedTo.Operator = Searcher.Ops[node.Children.Count + 1][j];
        //                Children.Add(new TreeOfTreesNode(clone, this.Size + 1, this));
        //            }
        //        }
        //    }
        //}

        public bool Equals(IState other)
        {
            return ((TreeOfTreesNode)other).FunctionTreeRoot.Equals(this.FunctionTreeRoot);
        }
    }
}
