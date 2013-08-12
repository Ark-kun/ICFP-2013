using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ark.Icfp2013.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace Ark.Icfp2013 {
    class Program {
        static void Main(string[] args) {
            var allProblems = Server.MyProblems();

            var unsolvedProblems = allProblems.Where(p => p.Solved != true).ToArray();

            var noTimeProblems = unsolvedProblems.Where(p => !p.TimeLeft.HasValue).ToArray();

            var convertedProblems = noTimeProblems.Select(Server.ParseProblem).ToList();
            var bonusProblems = convertedProblems.Where(p => (p.UsedOperators & OperatorType.Bonus) != OperatorType.None).OrderBy(p => p.Size).ToArray();

            //return;
            //var inputs = InputsGenerator.GetInputs(1024);
            var inputs = InputsGenerator.GetInputs(256);
            var inputsDictionary = new Dictionary<ulong, int>();
            for (int i = 0; i < inputs.Length; i++) {
                inputsDictionary.Add(inputs[i], i);
            }

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

            int maxLevel = 9;

            var unaryFactories = new Func<FormulaNode, FormulaNode>[] { Shl1.Create, Shr1.Create, Shr4.Create, Shr16.Create, Not.Create };
            var binaryFactories = new Func<FormulaNode, FormulaNode, FormulaNode>[] { And.Create, Or.Create, Xor.Create, Plus.Create };
            var ternaryFactories = new Func<FormulaNode, FormulaNode, FormulaNode, FormulaNode>[] { If0.Create };

            FormulasGenerator generator = new FormulasGenerator();
            generator.GenerateFormulas(zero, one, argX, unaryFactories, binaryFactories, ternaryFactories, maxLevel);

            var formulasByLevel = generator.FormulasByLevel;

            using (var dump = File.CreateText("formulas.txt")) {
                for (int level = 1; level < formulasByLevel.Count; level++) {
                    Console.WriteLine("Count(size = {0}) = {1}", level, formulasByLevel[level].Count);

                    foreach (var f in formulasByLevel[level]) {
                        dump.WriteLine(f);
                    }
                }
            }

            foreach (var problemToSolve in bonusProblems) {
                //Problem problemToSolve = null;
                //string id = null;
                //string file = id + ".problem";
                //if (id == null || !File.Exists(file)) {
                //    //problemToSolve = Server.GetTrainProblem(ProblemType.Normal, 15);
                //    problemToSolve = Server.GetTrainProblem(ProblemType.Bonus, -1);
                //    id = problemToSolve.ID;
                //    file = id + ".problem";
                //    //var problem = new Problem() {ID = ""};

                var results = Server.EvalProblemOnAllInputs(problemToSolve.ID, inputs);
                problemToSolve.EvalResults = results;
                //    using (var writer = File.CreateText(file)) {
                //        writer.WriteLine("id=" + problemToSolve.ID);
                //        writer.WriteLine("size=" + problemToSolve.Size);
                //        writer.WriteLine("solution=" + problemToSolve.Solution);
                //        writer.WriteLine("operators=" + problemToSolve.UsedOperators);
                //        writer.WriteLine("results=");
                //        foreach (var kv in problemToSolve.EvalResults) {
                //            writer.WriteLine("{0}={1}", kv.Key, kv.Value);
                //        }
                //    }
                //} else {
                //    using (var reader = File.OpenText(file)) {

                //    }
                //}

                Log(string.Format("Trying problem {0}", problemToSolve.ID));

                var isWin = SolveBonusProblem(inputs, inputsDictionary, generator, problemToSolve);
                Log(string.Format("{0}: {1}", problemToSolve.ID, (isWin ? "win" : "fail")));
            }

            Console.ReadLine();
        }

        static void Log(string msg) {
            Console.WriteLine(msg);
            File.AppendAllText("solve.log", msg + "\n");
        }


        private static bool SolveBonusProblem(ulong[] inputs, Dictionary<ulong, int> inputsDictionary, FormulasGenerator generator, Problem problemToSolve) {
            int inputsCount = inputs.Length;
            ulong[] orderedEvalResults = new ulong[inputs.Length];
            foreach (var kv in problemToSolve.EvalResults) {
                orderedEvalResults[inputsDictionary[kv.Key]] = kv.Value;
            }
            var problemNode = new KnownResultsNode() { Results = orderedEvalResults };
            var problemHash = problemNode.GetHashCode();

            var isKnown = generator.AllFormulas.Contains(problemNode);

            var guesses = new List<FormulaNode>();

            if (isKnown) {
                guesses.AddRange(generator.AllFormulas.Where(p => p.Equals(problemNode)));
            } else {
                //var known = generator.AllFormulas.Where(p => p.GetHashCode() == problemHash).ToArray();

                var formulasArray = generator.AllFormulas.ToArray();
                int count = formulasArray.Length;
                var fitnesses = new int[count];
                for (int i = 0; i < formulasArray.Length; i++) {
                    int fitness = 0;
                    var knownFormulaResults = formulasArray[i].Results;
                    for (int j = 0; j < inputsCount; j++) {
                        if (orderedEvalResults[j] == knownFormulaResults[j]) {
                            fitness++;
                        }
                    }
                    fitnesses[i] = fitness;
                }
                var sortedFitnesses = fitnesses.Select((f, i) => new { Index = i, Fitness = f }).OrderByDescending(kv => kv.Fitness).ToArray();
                var bestFormulaIdx = sortedFitnesses.First().Index;
                var bestFormula = formulasArray[bestFormulaIdx];
                var bestFormulaResults = bestFormula.Results;

                var mismatchedInputIndexes = new List<int>();
                for (int j = 0; j < inputsCount; j++) {
                    if (orderedEvalResults[j] != bestFormulaResults[j]) {
                        mismatchedInputIndexes.Add(j);
                    }
                }
                var complimentaryFormulas = new List<FormulaNode>();
                var complimentaryFormulaIndexes = new List<int>();
                for (int i = 0; i < formulasArray.Length; i++) {
                    var formulaResults = formulasArray[i].Results;
                    bool isGood = true;
                    foreach (var mismatchedInputIndex in mismatchedInputIndexes) {
                        if (formulaResults[mismatchedInputIndex] != orderedEvalResults[mismatchedInputIndex]) {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) {
                        complimentaryFormulas.Add(formulasArray[i]);
                        complimentaryFormulaIndexes.Add(i);
                    }
                }
                var sortedComplimentaryFormulas = complimentaryFormulaIndexes.Select((idx) => new { Formula = formulasArray[idx], Fitness = fitnesses[idx] }).OrderByDescending(kv => kv.Fitness).ToArray();

                if (sortedComplimentaryFormulas.Any()) {
                    var bestComplimentaryFormula = sortedComplimentaryFormulas.First().Formula;
                    var bestComplimentaryFormulaResults = bestComplimentaryFormula.Results;
                    //Searching for condition

                    var canBeTrue = new bool[inputsCount];
                    var canBeFalse = new bool[inputsCount];

                    for (int j = 0; j < inputsCount; j++) {
                        if (bestFormulaResults[j] == orderedEvalResults[j]) {
                            canBeTrue[j] = true;
                        }
                        if (bestComplimentaryFormulaResults[j] == orderedEvalResults[j]) {
                            canBeFalse[j] = true;
                        }
                    }

                    var conditionFormulas = new List<FormulaNode>();
                    var antiConditionFormulas = new List<FormulaNode>();

                    for (int i = 0; i < formulasArray.Length; i++) {
                        var formulaResults = formulasArray[i].Results;

                        bool isGood = true;
                        for (int j = 0; j < inputsCount; j++) {
                            if (!((formulaResults[j] == 0 && canBeTrue[j]) || (formulaResults[j] != 0 && canBeFalse[j]))) {
                                isGood = false;
                                break;
                            }
                        }
                        if (isGood) {
                            conditionFormulas.Add(formulasArray[i]);
                        }

                        bool isAntiGood = true;
                        for (int j = 0; j < inputsCount; j++) {
                            if (!((formulaResults[j] == 0 && canBeFalse[j]) || (formulaResults[j] != 0 && canBeTrue[j]))) {
                                isAntiGood = false;
                                break;
                            }
                        }
                        if (isAntiGood) {
                            antiConditionFormulas.Add(formulasArray[i]);
                        }
                    }

                    foreach (var condition in conditionFormulas) {
                        guesses.Add(new If0(condition, bestFormula, bestComplimentaryFormula));
                    }
                    foreach (var condition in antiConditionFormulas) {
                        guesses.Add(new If0(condition, bestComplimentaryFormula, bestFormula));
                    }
                }
            }

            if (guesses.Any()) {
                Log(string.Format("{0}: Guesses: {1}", problemToSolve.ID, guesses.Count));
                var guessFormula = guesses.First();
                var rootFormula = new RootLambda(guessFormula);
                Log(string.Format("{0}: GuessFormula: {1}", problemToSolve.ID, rootFormula));
                return Server.SubmitGuess(problemToSolve.ID, rootFormula);
            } else {
                Log(string.Format("{0}: No guesses", problemToSolve.ID));
                return false;
            }
        }

    }
}
