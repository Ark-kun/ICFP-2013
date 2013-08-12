using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Icfp2013 {
    public class InputsGenerator {
        public static ulong[] GetInputs(int n) {
            if (File.Exists("randoms2.txt")) {
                return File.ReadAllLines("randoms2.txt").Select(a => ulong.Parse(a, System.Globalization.NumberStyles.HexNumber)).ToArray();
            } else {
                Random rng = new Random(3137);
                var inputs = GetUlongsForEval(128).Concat(Enumerable.Range(0, n - 128).Select(a => GetRandomUlong(rng))).ToArray();
                inputs[n - 1] = 0;
                File.WriteAllLines("randoms2.txt", inputs.Select(a => a.ToString("X")));
                return inputs;
            }
        }

        static ulong[] GetUlongsForEval(int num) {
            ulong[] res = new ulong[num];
            ulong c = 1;
            for (int i = 0; i < num; i += 2) {
                res[i] = c;
                res[i + 1] = ~c;
                c = c << 1;
            }

            return res;
        }

        static ulong GetRandomUlong(Random rng) {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }
    }
}
