using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Icfp2013 {
    public static class ArrayEqualityHelpers {
        public static bool EqualsTo(this ulong[] a, ulong[] b) {
            if (a == null && b == null) {
                return true;
            }
            if (a == null || b == null) {
                return false;
            }
            if (a.Length != b.Length) {
                return false;
            }

            for (int i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }
            return true;
        }

        //public static int DumbHash(this ulong[] values) {
        //    int sum = 0x5bd1e995;
        //    foreach (var value in values) { 
                
        //    }
        //}

        public static int MurmurHash3(this ulong[] values) {
            uint seed = 0;
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            const int r1 = 15;
            const int r1m = 32 - r1;
            const int r2 = 13;
            const int r2m = 32 - r2;
            const uint m = 5;
            const uint n = 0xe6546b64;

            uint hash = seed;

            foreach (var value in values) {
                uint k = (uint)(value >> 32);
                k *= c1;
                k = (k << r1) | (k >> r1m);
                k *= c2;

                hash ^= k;
                hash = (hash << r2) | (hash >> r2m);
                hash *= m;
                hash += n;

                k = (uint)value;
                k *= c1;
                k = (k << r1) | (k >> r1m);
                k *= c2;

                hash ^= k;
                hash = (hash << r2) | (hash >> r2m);
                hash *= m;
                hash += n;
            }
            hash ^= (uint)values.Length * 8;
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;
            return unchecked((int)hash);
        }

        public static int MurmurHash2(this ulong[] values) {
            byte[] source = new byte[values.Length * 8];

            for (int i = 0; i < values.Length; i++) {
                for (int j = 0; j < 8; j++) {
                    source[i * 8 + j] = (byte)(values[i] >> j * 8);
                }
            }

            return (int)MurmurHash2(source);
        }

        public static uint MurmurHash2(byte[] source) {
            uint len = (uint)source.Length;
            uint m = 0x5bd1e995;
            uint seed = 0;
            int r = 24;
            uint h = seed ^ len;

            int offset = 0;
            while (len >= 4) {
                uint k;
                k = source[offset];
                k |= (uint)source[offset + 1] << 8;
                k |= (uint)source[offset + 2] << 16;
                k |= (uint)source[offset + 3] << 24;

                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;

                offset += 4;
                len -= 4;
            }

            switch (len) {
                case 3:
                    h ^= (uint)source[offset + 2] << 16;
                    goto case 2;
                case 2:
                    h ^= (uint)source[offset + 1] << 8;
                    goto case 1;
                case 1:
                    h ^= (uint)source[offset];
                    h *= m;
                    break;
            };

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;

        }
    }
}
