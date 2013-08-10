using System.Collections.Generic;

namespace Ark.Icfp2013 {

    //struct Context {
    //    ulong X;
    //    ulong Accumulator;
    //    ulong CurrentByte;
    //}

    public abstract class FormulaNode {
        Dictionary<ulong, ulong> _cache;

        public ulong Evaluate(ulong x) {
            ulong result = 0;
            if (!_cache.TryGetValue(x, out result)) {
                result = EvaluateInternal(x);
                _cache[x] = result;
            }
            return result;
        }

        protected abstract ulong EvaluateInternal(ulong x);

        public abstract override string ToString();
    }

    public abstract class NullaryNode : FormulaNode {
    }

    public abstract class UnaryNode : FormulaNode {
        protected FormulaNode _arg;

        public UnaryNode(FormulaNode arg) {
            _arg = arg;
        }
    }

    public abstract class BinaryNode : FormulaNode {
        protected FormulaNode _arg1;
        protected FormulaNode _arg2;

        public BinaryNode(FormulaNode arg1, FormulaNode arg2) {
            _arg1 = arg1;
            _arg2 = arg2;
        }
    }

    public abstract class TernaryNode : FormulaNode {
        protected FormulaNode _arg1;
        protected FormulaNode _arg2;
        protected FormulaNode _arg3;

        public TernaryNode(FormulaNode arg1, FormulaNode arg2, FormulaNode arg3) {
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }
    }

    //Nullary
    public class Zero : NullaryNode {
        public static FormulaNode Create() { 
            return new Zero();
        }

        protected override ulong EvaluateInternal(ulong x) {
            return 0;
        }

        public override string ToString() {
            return "0";
        }
    }

    public class One : NullaryNode {
        public static FormulaNode Create() {
            return new One();
        }

        protected override ulong EvaluateInternal(ulong x) {
            return 1;
        }

        public override string ToString() {
            return "1";
        }
    }
    
    public class ArgX : NullaryNode { //?
        public static FormulaNode Create() {
            return new ArgX();
        }

        protected override ulong EvaluateInternal(ulong x) {
            return x;
        }

        public override string ToString() {
            return "x";
        }
    }

    //Unary
    public class Shl1 : UnaryNode {
        public Shl1(FormulaNode arg) : base(arg) { }

        public static FormulaNode Create(FormulaNode arg) {
            if (arg is Zero) {
                return null;
            }
            return new Shl1(arg);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg.Evaluate(x) << 1;
        }

        public override string ToString() {
            return "(shl1 " + _arg.ToString() + ")";
        }
    }

    public class Shr1 : UnaryNode {
        public Shr1(FormulaNode arg) : base(arg) { }

        public static FormulaNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr1(arg);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg.Evaluate(x) >> 1;
        }

        public override string ToString() {
            return "(shr1 " + _arg.ToString() + ")";
        }
    }

    public class Shr4 : UnaryNode {
        public Shr4(FormulaNode arg) : base(arg) { }

        public static FormulaNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr4(arg);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg.Evaluate(x) >> 4;
        }

        public override string ToString() {
            return "(shr4 " + _arg.ToString() + ")";
        }
    }

    public class Shr16 : UnaryNode {
        public Shr16(FormulaNode arg) : base(arg) { }

        public static FormulaNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr16(arg);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg.Evaluate(x) >> 16;
        }

        public override string ToString() {
            return "(shr16 " + _arg.ToString() + ")";
        }
    }

    public class Not : UnaryNode {
        public Not(FormulaNode arg) : base(arg) { }

        public static FormulaNode Create(FormulaNode arg) {
            if (arg is Not) {
                return null;
            }
            return new Not(arg);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return ~_arg.Evaluate(x);
        }

        public override string ToString() {
            return "(not " + _arg.ToString() + ")";
        }
    }

    //Binary
    public class And : BinaryNode {
        public And(FormulaNode arg1, FormulaNode arg2) : base(arg1, arg2) { }

        public static FormulaNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero) {
                return null;
            }
            return new And(arg1, arg2);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) & _arg2.Evaluate(x);
        }

        public override string ToString() {
            return "(and " + _arg1.ToString() + " " + _arg2.ToString() + ")";
        }
    }

    public class Or : BinaryNode {
        public Or(FormulaNode arg1, FormulaNode arg2) : base(arg1, arg2) { }

        public static FormulaNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero) {
                return null;
            }
            return new Or(arg1, arg2);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) | _arg2.Evaluate(x);
        }

        public override string ToString() {
            return "(or " + _arg1.ToString() + " " + _arg2.ToString() + ")";
        }
    }

    public class Xor : BinaryNode {
        public Xor(FormulaNode arg1, FormulaNode arg2) : base(arg1, arg2) { }

        public static FormulaNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero) {
                return null;
            }
            return new Xor(arg1, arg2);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) ^ _arg2.Evaluate(x);
        }

        public override string ToString() {
            return "(xor " + _arg1.ToString() + " " + _arg2.ToString() + ")";
        }
    }

    public class Plus : BinaryNode {
        public Plus(FormulaNode arg1, FormulaNode arg2) : base(arg1, arg2) { }

        public static FormulaNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero) {
                return null;
            }
            return new Plus(arg1, arg2);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) + _arg2.Evaluate(x);
        }

        public override string ToString() {
            return "(plus " + _arg1.ToString() + " " + _arg2.ToString() + ")";
        }
    }

    //Ternary
    public class If0 : TernaryNode {
        public If0(FormulaNode arg1, FormulaNode arg2, FormulaNode arg3) : base(arg1, arg2, arg3) { }

        public static FormulaNode Create(FormulaNode arg1, FormulaNode arg2, FormulaNode arg3) {
            return new If0(arg1, arg2, arg3);
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) == 0 ? _arg2.Evaluate(x) : _arg3.Evaluate(x);
        }

        public override string ToString() {
            return "(if0 " + _arg1.ToString() + " " + _arg2.ToString() + " " + _arg3.ToString() + ")";
        }
    }
}
