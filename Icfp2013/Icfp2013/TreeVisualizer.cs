using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class TreeVisualizer
    {        
        public string Visualize(FunctionTreeNode tree)
        {         
            return VisualizeInternal(tree);
        }

        string VisualizeInternal(FunctionTreeNode tree)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(tree.Operator.ToString());            

            if (tree.Children.Count > 0)
            {               
                sb.Append("(" + string.Join(",", tree.Children.Select(a => VisualizeInternal(a)).ToArray()) + ")");               
            }

            return sb.ToString();
        }
    }
}
