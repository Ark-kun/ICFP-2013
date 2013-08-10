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

        public override string ToString()
        {
            return "x";
        }
    }

    class Zero : Op
    {
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] arg)
        {
            return 0;
        }

        public override string ToString()
        {
            return "0";
        }
    }

    class One : Op
    {
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] arg)
        {
            return 1;
        }

        public override string ToString()
        {
            return "1";
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

        public override string ToString()
        {
            return "Shl1";
        }
    }

    class Shr1 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 1;
        }

        public override string ToString()
        {
            return "Shr1";
        }
    }

    class Shr4 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 4;
        }

        public override string ToString()
        {
            return "Shr4";
        }
    }

    class Shr16 : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] >> 16;
        }

        public override string ToString()
        {
            return "Shr16";
        }
    }

    class Not : Op
    {
        public int Arity { get { return 1; } }
        public ulong Eval(ulong[] arg)
        {
            return ~arg[0];
        }

        public override string ToString()
        {
            return "Not";
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

        public override string ToString()
        {
            return "And";
        }
    }

    class Or : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] | arg[1];
        }

        public override string ToString()
        {
            return "Or";
        }
    }

    class Xor : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] ^ arg[1];
        }

        public override string ToString()
        {
            return "Xor";
        }
    }

    class Plus : Op
    {
        public int Arity { get { return 2; } }
        public ulong Eval(ulong[] arg)
        {
            return arg[0] + arg[1];
        }

        public override string ToString()
        {
            return "Plus";
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

        public override string ToString()
        {
            return "If0";
        }
    }
}
