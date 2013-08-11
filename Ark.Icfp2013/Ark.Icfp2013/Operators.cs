using System.Collections.Generic;

namespace Ark.Icfp2013 {

    //struct Context {
    //    ulong X;
    //    ulong Accumulator;
    //    ulong CurrentByte;
    //}

    public abstract class FormulaNode {
        protected ulong[] _results;
        int? hashCode;

        public ulong[] Results {
            get { return GetResults(); }
            set { _results = value; }
        }

        //Dictionary<ulong, ulong> _cache;

        public ulong Evaluate(ulong x) {
            return EvaluateInternal(x);
            //ulong result = 0;
            //if (!_cache.TryGetValue(x, out result)) {
            //    result = EvaluateInternal(x);
            //    _cache[x] = result;
            //}
            //return result;
        }

        protected abstract ulong EvaluateInternal(ulong x);

        public void CacheResults() {
            _results = GetResults();
        }

        public abstract ulong[] GetResults();

        public abstract override string ToString();

        public override bool Equals(object obj) {
            var otherFormula = obj as FormulaNode;
            return otherFormula != null && this.GetResults().EqualsTo(otherFormula.GetResults());
        }

        public override int GetHashCode() {
            if (!hashCode.HasValue) {
                hashCode = this.GetResults().MurmurHash3();
            }
            return hashCode.Value;
        }
    }

    public class KnownResultsNode : FormulaNode {
        protected override ulong EvaluateInternal(ulong x) {
            throw new System.NotImplementedException();
        }

        public override ulong[] GetResults() {
            return _results;
        }

        public override string ToString() {
            throw new System.NotImplementedException();
        }
    }

    public abstract class NullaryNode : FormulaNode {
        public override ulong[] GetResults() {
            return _results;
        }
    }

    public abstract class UnaryNode : FormulaNode {
        protected FormulaNode _arg;

        public abstract ulong EvaluateOperator(ulong arg);

        public UnaryNode(FormulaNode arg) {
            _arg = arg;
        }

        public override ulong[] GetResults() {
            if (_results != null) {
                return _results;
            }
            var argResults = _arg.GetResults();
            var results = new ulong[argResults.Length];
            for (int i = 0; i < argResults.Length; i++) {
                results[i] = EvaluateOperator(argResults[i]);
            }
            return results;
        }
    }

    public abstract class BinaryNode : FormulaNode {
        protected FormulaNode _arg1;
        protected FormulaNode _arg2;

        public abstract ulong EvaluateOperator(ulong arg1, ulong arg2);

        public BinaryNode(FormulaNode arg1, FormulaNode arg2) {
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public override ulong[] GetResults() {
            if (_results != null) {
                return _results;
            }
            var arg1Results = _arg1.GetResults();
            var arg2Results = _arg2.GetResults();
            var results = new ulong[arg1Results.Length];
            for (int i = 0; i < arg1Results.Length; i++) {
                results[i] = EvaluateOperator(arg1Results[i], arg2Results[i]);
            }
            return results;
        }
    }

    public abstract class TernaryNode : FormulaNode {
        protected FormulaNode _arg1;
        protected FormulaNode _arg2;
        protected FormulaNode _arg3;

        public abstract ulong EvaluateOperator(ulong arg1, ulong arg2, ulong arg3);

        public TernaryNode(FormulaNode arg1, FormulaNode arg2, FormulaNode arg3) {
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        public override ulong[] GetResults() {
            if (_results != null) {
                return _results;
            }
            var arg1Results = _arg1.GetResults();
            var arg2Results = _arg2.GetResults();
            var arg3Results = _arg2.GetResults();
            var results = new ulong[arg1Results.Length];
            for (int i = 0; i < arg1Results.Length; i++) {
                results[i] = EvaluateOperator(arg1Results[i], arg2Results[i], arg3Results[i]);
            }
            return results;
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

        public static UnaryNode Create(FormulaNode arg) {
            if (arg is Zero) {
                return null;
            }
            return new Shl1(arg);
        }

        public override ulong EvaluateOperator(ulong arg) {
            return arg << 1;
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

        public static UnaryNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr1(arg);
        }

        public override ulong EvaluateOperator(ulong arg) {
            return arg >> 1;
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

        public static UnaryNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr4(arg);
        }

        public override ulong EvaluateOperator(ulong arg) {
            return arg >> 4;
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

        public static UnaryNode Create(FormulaNode arg) {
            if (arg is Zero || arg is One) {
                return null;
            }
            return new Shr16(arg);
        }

        public override ulong EvaluateOperator(ulong arg) {
            return arg >> 16;
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

        public static UnaryNode Create(FormulaNode arg) {
            if (arg is Not) {
                return null;
            }
            return new Not(arg);
        }

        public override ulong EvaluateOperator(ulong arg) {
            return ~arg;
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

        public static BinaryNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero || object.ReferenceEquals(arg1, arg2) || (arg1 is One && arg2 is Shl1) || (arg2 is One && arg1 is Shl1)) {
                return null;
            }
            return new And(arg1, arg2);
        }

        public override ulong EvaluateOperator(ulong arg1, ulong arg2) {
            return arg1 & arg2;
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

        public static BinaryNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero || object.ReferenceEquals(arg1, arg2)) {
                return null;
            }
            return new Or(arg1, arg2);
        }

        public override ulong EvaluateOperator(ulong arg1, ulong arg2) {
            return arg1 | arg2;
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

        public static BinaryNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero || object.ReferenceEquals(arg1, arg2)) {
                return null;
            }
            return new Xor(arg1, arg2);
        }

        public override ulong EvaluateOperator(ulong arg1, ulong arg2) {
            return arg1 ^ arg2;
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

        public static BinaryNode Create(FormulaNode arg1, FormulaNode arg2) {
            if (arg1 is Zero || arg2 is Zero) {
                return null;
            }
            return new Plus(arg1, arg2);
        }

        public override ulong EvaluateOperator(ulong arg1, ulong arg2) {
            return arg1 + arg2;
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

        public static TernaryNode Create(FormulaNode arg1, FormulaNode arg2, FormulaNode arg3) {
            return new If0(arg1, arg2, arg3);
        }

        public override ulong EvaluateOperator(ulong arg1, ulong arg2, ulong arg3) {
            return arg1 == 0 ? arg2 : arg3;
        }

        protected override ulong EvaluateInternal(ulong x) {
            return _arg1.Evaluate(x) == 0 ? _arg2.Evaluate(x) : _arg3.Evaluate(x);
        }

        public override string ToString() {
            return "(if0 " + _arg1.ToString() + " " + _arg2.ToString() + " " + _arg3.ToString() + ")";
        }
    }
}
