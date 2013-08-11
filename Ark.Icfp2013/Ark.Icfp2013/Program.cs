using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Icfp2013 {
    class Program {
        static void Main(string[] args) {
            var inputs = InputsGenerator.GetInputs();

            var formulasByLevel = new List<List<FormulaNode>>();
            var allFormulas = new HashSet<FormulaNode>();

            int maxLevel = 10;

            //var nullaryOps = new FormulaNode[] { Zero, One, ArgX };
            var formulasLevel0 = new List<FormulaNode>();
            formulasByLevel.Add(formulasLevel0);

            var zero = new Zero();
            zero.Results = new ulong[inputs.Length];
            var one = new One();
            one.Results = new ulong[inputs.Length];
            for (int i = 0; i < inputs.Length; i++) {
                one.Results[i] = 1;
            }
            var argX = new ArgX();
            argX.Results = new ulong[inputs.Length];
            for (int i = 0; i < inputs.Length; i++) {
                argX.Results[i] = inputs[i];
            }

            var formulasLevel1 = new List<FormulaNode> { zero, one, argX };
            formulasByLevel.Add(formulasLevel1);

            var unaryFactories = new Func<FormulaNode, FormulaNode>[] { Shl1.Create, Shr1.Create, Shr16.Create, Not.Create };
            var binaryFactories = new Func<FormulaNode, FormulaNode, FormulaNode>[] { And.Create, Or.Create, Xor.Create, Plus.Create };

            try {
                for (int level = 2; level <= maxLevel; level++) {
                    var formulas = new List<FormulaNode>();
                    //Unary
                    int unaryArgLevel = level - 1;
                    if (unaryArgLevel > 0) {
                        foreach (var arg in formulasByLevel[unaryArgLevel]) {
                            foreach (var factory in unaryFactories) {
                                var formula = factory(arg);
                                if (formula != null) {
                                    formula.FillResults();
                                    if (allFormulas.Add(formula)) {
                                        formulas.Add(formula);
                                    } else {
                                    }
                                }
                            }
                        }
                    }
                    //Binary
                    for (int binaryArg1Level = 1; binaryArg1Level < level; binaryArg1Level++) {
                        int binaryArg2Level = level - binaryArg1Level - 1;
                        if (binaryArg2Level > 0 && binaryArg2Level >= binaryArg1Level) { //Preventing duplicates (binary operators are commutative)
                            var formulas1 = formulasByLevel[binaryArg1Level];
                            var formulas2 = formulasByLevel[binaryArg2Level];
                            for (int arg1Idx = 0; arg1Idx < formulas1.Count; arg1Idx++) {
                                int arg2IdxStart = (binaryArg2Level == binaryArg1Level ? arg1Idx : 0); //Preventing duplicates (binary operators are commutative)
                                for (int arg2Idx = arg2IdxStart; arg2Idx < formulas2.Count; arg2Idx++) {
                                    var arg1 = formulas1[arg1Idx];
                                    var arg2 = formulas2[arg2Idx];
                                    foreach (var factory in binaryFactories) {
                                        var formula = factory(arg1, arg2);
                                        if (formula != null) {
                                            formula.FillResults();
                                            if (allFormulas.Add(formula)) {
                                                formulas.Add(formula);
                                            } else {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    formulasByLevel.Add(formulas);
                }
            } catch (OutOfMemoryException ex) {
            }

            for (int level = 1; level <= maxLevel; level++) {
                Console.WriteLine("Count(size = {0}) = {1}", level, formulasByLevel[level].Count);
            }
            Console.ReadLine();
        }
    }
}
