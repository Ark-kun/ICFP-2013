using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013.Operators
{
    interface Op
    {
        int Arity { get; }
        ulong Eval(params ulong[] arg);
    }

    //Nullary
    class Arg : Op
    {
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0];
        }
    }

    class Zero : Op
    {
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] arg)
        {
            return 0;
        }
    }

    class One : Op
    {
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] arg)
        {
            return 1;
        }
    }

    //Unary
    class Shl1 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] << 1;
        }
    }

    class Shr1 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 1;
        }
    }

    class Shr4 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 4;
        }
    }

    class Shr16 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 16;
        }
    }

    class Not : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return ~arg[0];
        }
    }

    //Binary
    class And : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] & arg[1];
        }
    }

    class Or : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] | arg[1];
        }
    }

    class Xor : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] ^ arg[1];
        }
    }

    class Plus : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] + arg[1];
        }
    }

    //Ternary
    class If0 : Op
    {
        public int Arity { get { return 3; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] == 0 ? arg[1] : arg[2];
        }
    }
}
