using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class TreeOfTreesNode
    {
        static int MaxArity = 3;

        public int Size;
        public FunctionTreeNode FunctionTreeRoot;
        public List<TreeOfTreesNode> Children;
        public TreeOfTreesNode Parent;

        public TreeOfTreesNode(FunctionTreeNode root, int size, TreeOfTreesNode parent = null)
        {
            this.FunctionTreeRoot = root;
            this.Size = size;
            this.Parent = parent;
        }

        public void CreateChildren()
        {
            Children = new List<TreeOfTreesNode>();
            AddChildrenForNode(FunctionTreeRoot);
        }

        void AddChildrenForNode(FunctionTreeNode node)
        {
            foreach (var ch in node.Children)
            {
                AddChildrenForNode(ch);
            }

            if (node.Children.Count < MaxArity)
            {
                FunctionTreeNode clone = FunctionTreeRoot.Clone();
                node.LastClonedTo.Children.Add(new FunctionTreeNode(clone.Context, node.LastClonedTo));
                Children.Add(new TreeOfTreesNode(clone, this.Size + 1, this));
            }
        }
    }
}
