using System;
using System.Collections.Generic;

namespace Ark.Icfp2013 {
    public class FormulasGenerator {
        List<List<FormulaNode>> _formulasByLevel;
        HashSet<FormulaNode> _allFormulas;

        public List<List<FormulaNode>> FormulasByLevel { get { return _formulasByLevel; } }

        public HashSet<FormulaNode> AllFormulas { get { return _allFormulas; } }

        public List<List<FormulaNode>> GenerateFormulas(Zero zero, One one, ArgX argX, Func<FormulaNode, FormulaNode>[] unaryFactories, Func<FormulaNode, FormulaNode, FormulaNode>[] binaryFactories, Func<FormulaNode, FormulaNode, FormulaNode, FormulaNode>[] ternaryFactories, int maxLevel) {
            _formulasByLevel = new List<List<FormulaNode>>();
            _allFormulas = new HashSet<FormulaNode>();

            //var nullaryOps = new FormulaNode[] { Zero, One, ArgX };
            var formulasLevel0 = new List<FormulaNode>();
            _formulasByLevel.Add(formulasLevel0);


            var formulasLevel1 = new List<FormulaNode> { zero, one, argX };
            _formulasByLevel.Add(formulasLevel1);


            try {
                for (int level = 2; level <= maxLevel; level++) {
                    var formulas = new List<FormulaNode>();
                    //Unary
                    int unaryArgLevel = level - 1;
                    if (unaryArgLevel > 0) {
                        foreach (var arg in _formulasByLevel[unaryArgLevel]) {
                            foreach (var factory in unaryFactories) {
                                var formula = factory(arg);
                                if (formula != null) {
                                    if (level <= 10) {
                                        formula.CacheResults();
                                    }
                                    if (_allFormulas.Add(formula)) {
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
                            var formulas1 = _formulasByLevel[binaryArg1Level];
                            var formulas2 = _formulasByLevel[binaryArg2Level];
                            for (int arg1Idx = 0; arg1Idx < formulas1.Count; arg1Idx++) {
                                int arg2IdxStart = (binaryArg2Level == binaryArg1Level ? arg1Idx : 0); //Preventing duplicates (binary operators are commutative)
                                for (int arg2Idx = arg2IdxStart; arg2Idx < formulas2.Count; arg2Idx++) {
                                    var arg1 = formulas1[arg1Idx];
                                    var arg2 = formulas2[arg2Idx];
                                    foreach (var factory in binaryFactories) {
                                        var formula = factory(arg1, arg2);
                                        if (formula != null) {
                                            if (level <= 10) {
                                                formula.CacheResults();
                                            }
                                            if (_allFormulas.Add(formula)) {
                                                formulas.Add(formula);
                                            } else {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("Count(size = {0}) = {1}; {2}", level, formulas.Count, DateTime.UtcNow);
                    _formulasByLevel.Add(formulas);
                }
            } catch (OutOfMemoryException ex) {
                Console.WriteLine(ex);
            }
            return _formulasByLevel;
        }

    }
}
