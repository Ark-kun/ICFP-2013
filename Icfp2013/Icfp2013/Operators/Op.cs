using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013.Operators
{
    interface Op
    {
        EvaluationContext Context { set; }        
        int Arity { get; }
        ulong Eval(params ulong[] arg);
    }

    //Nullary
    class Arg : Op
    {
        public EvaluationContext Context { get; set; }     
        static string[] Letters = new[] { "x", "y", "z" };

        public int ArgIndex;
        public int Arity { get { return 0; } }
        public ulong Eval(ulong[] args)
        {
            switch (ArgIndex)
            {
                case 0: return Context.Arg;
                case 1: return Context.FoldY;
                case 2: return Context.FoldZ;
                default: throw new Exception("WTF");
            }
        }        

        public override string ToString()
        {
            return Letters[ArgIndex];
        }

        public Arg(int i)
        {
            ArgIndex = i;
        }          
    }
    

    class Zero : Op
    {
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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
        public EvaluationContext Context { get; set; }  
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

    class Fold : Op
    {
        public EvaluationContext Context { get; set; }  
        public int Arity { get { return 3; } }
        public ulong Eval(ulong[] arg)
        {
            throw new Exception("Can't eval");
        }

        public ulong FoldEval(ulong y, ulong z, FunctionTreeNode func)
        {
            Context.FoldZ = z;
            for (int i = 0; i < 8; i++)
            {
                ulong b = (y >> i * 8) & 0xFF;
                Context.FoldY = b;
                Context.FoldZ = func.Eval();
            }

            return Context.FoldZ;
        }

        public override string ToString()
        {
            return "Fold";
        }
    }
}
