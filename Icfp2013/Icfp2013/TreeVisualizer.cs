using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class TreeVisualizer
    {
        int currentId;

        public string Visualize(FunctionTreeNode tree)
        {
            currentId = 0;

            return VisualizeInternal(tree);
        }

        string VisualizeInternal(FunctionTreeNode tree)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(currentId.ToString());
            currentId++;

            if (tree.Children.Count > 0)
            {               
                sb.Append("(" + string.Join(",", tree.Children.Select(a => VisualizeInternal(a)).ToArray()) + ")");               
            }

            return sb.ToString();
        }
    }
}
