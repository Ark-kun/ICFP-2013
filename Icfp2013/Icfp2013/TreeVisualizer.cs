using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class TreeVisualizer
    {        
        public string Visualize(FunctionTreeNode tree, bool cacheOpShow)
        {
            return "(lambda (x) " + VisualizeInternal(tree, cacheOpShow) + ")";
        }

        string VisualizeInternal(FunctionTreeNode tree, bool cacheOpShow)
        {
            StringBuilder sb = new StringBuilder();
            if (tree.Operator.Arity > 0)
            {
                sb.Append("(");
            }


            if (cacheOpShow && tree.Operator is Operators.CachedOp)
            {
                sb.Append("*");
            }
            else
            {
                sb.Append(tree.Operator.ToString().ToLowerInvariant());
            }

            if (tree.Operator.Arity > 0)
            {
                sb.Append(" ");
            }

            if (tree.Children.Count > 0)
            {               
                sb.Append(string.Join(" ", tree.Children.Select((a,i) =>
                    {
                        if (tree.Operator is Operators.Fold && i == 2)
                        {
                            return "(lambda (y z) " + VisualizeInternal(a, cacheOpShow) + ")";
                        }
                        else
                        {
                            return VisualizeInternal(a, cacheOpShow);
                        }
                    }).ToArray()));               
            }

            if (tree.Operator.Arity > 0)
            {
                sb.Append(")");
            }

            return  sb.ToString();
        }
    }
}
