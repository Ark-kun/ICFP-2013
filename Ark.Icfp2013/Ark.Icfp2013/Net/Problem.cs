using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Icfp2013.Net {
    public class Problem {
        public string ID;
        public int Size;
        public Dictionary<ulong, ulong> EvalResults = new Dictionary<ulong, ulong>();
        public string Solution;

        public OperatorType UsedOperators;
    }
}
