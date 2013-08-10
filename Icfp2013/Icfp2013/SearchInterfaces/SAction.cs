using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class SAction : Dzugaru.Search.IAction
    {
        public TreeOfTreesNode Next;

        public override string ToString()
        {
            return Next.FunctionTreeRoot.ToString();
        }
    }
}
