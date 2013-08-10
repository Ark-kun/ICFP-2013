using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class Problem
    {
        public int Size;
        public ulong[][] Evals;
        public string Solution;

        public override string ToString()
        {            
            return Solution;
        }
    }
}
