﻿using System;
using System.Collections.Generic;

namespace Ark.Icfp2013 {
    [Serializable]
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

            _allFormulas.UnionWith(formulasLevel1);

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

                    //Ternary
                    foreach (var factory in ternaryFactories) {
                        for (int ternaryArg1Level = 1; ternaryArg1Level < level; ternaryArg1Level++) {
                            for (int ternaryArg2Level = 1; ternaryArg2Level < level; ternaryArg2Level++) {
                                int ternaryArg3Level = level - ternaryArg1Level - ternaryArg2Level - 1;
                                if (ternaryArg3Level > 0) {
                                    var formulas1 = _formulasByLevel[ternaryArg1Level];
                                    var formulas2 = _formulasByLevel[ternaryArg2Level];
                                    var formulas3 = _formulasByLevel[ternaryArg3Level];
                                    for (int arg1Idx = 0; arg1Idx < formulas1.Count; arg1Idx++) {
                                        int arg2IdxStart = 0; //ternary operators are not com mutative
                                        for (int arg2Idx = arg2IdxStart; arg2Idx < formulas2.Count; arg2Idx++) {
                                            int arg3IdxStart = 0; //ternary operators are not com mutative
                                            for (int arg3Idx = arg3IdxStart; arg3Idx < formulas3.Count; arg3Idx++) {
                                                var arg1 = formulas1[arg1Idx];
                                                var arg2 = formulas2[arg2Idx];
                                                var arg3 = formulas3[arg3Idx];

                                                var formula = factory(arg1, arg2, arg3);
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
