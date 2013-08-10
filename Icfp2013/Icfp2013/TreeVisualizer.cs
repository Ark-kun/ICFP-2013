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
            return "(lambda (x) " + VisualizeInternal(tree) + ")";
        }

        string VisualizeInternal(FunctionTreeNode tree)
        {
            StringBuilder sb = new StringBuilder();
            if (tree.Operator.Arity > 0)
            {
                sb.Append("(");
            }

            sb.Append(tree.Operator.ToString().ToLowerInvariant());

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
                            return "(lambda (y z) " + VisualizeInternal(a) + ")";
                        }
                        else
                        {
                            return VisualizeInternal(a);
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
