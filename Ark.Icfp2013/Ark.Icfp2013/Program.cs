using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Icfp2013 {
    class Program {
        static void Main(string[] args) {
            var inputs = InputsGenerator.GetInputs();

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

            int maxLevel = 15;

            var unaryFactories = new Func<FormulaNode, FormulaNode>[] { Shl1.Create, Shr1.Create, Shr16.Create, Not.Create };
            var binaryFactories = new Func<FormulaNode, FormulaNode, FormulaNode>[] { And.Create, Or.Create, Xor.Create, Plus.Create };
            var ternaryFactories = new Func<FormulaNode, FormulaNode, FormulaNode, FormulaNode>[] { If0.Create };

            var generator = new FormulasGenerator();
            generator.GenerateFormulas(zero, one, argX, unaryFactories, binaryFactories, ternaryFactories, maxLevel);
            var formulasByLevel = generator.FormulasByLevel;

            for (int level = 1; level <= formulasByLevel.Count; level++) {
                Console.WriteLine("Count(size = {0}) = {1}", level, formulasByLevel[level].Count);
            }
            Console.ReadLine();
        }

    }
}
