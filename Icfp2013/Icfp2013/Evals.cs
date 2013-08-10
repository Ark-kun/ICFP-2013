using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    class Evals : IEquatable<Evals>
    {
        public ulong[] Values;

        public bool Equals(Evals other)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] != other.Values[i]) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals((Evals)obj);
        }

        public override int GetHashCode()
        {
            byte[] source = new byte[Values.Length * 8];

            for (int i = 0; i < Values.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    source[i * 8 + j] = (byte)(Values[i] >> j * 8);
                }
            }
            
            return (int)MurmurHash2(source);
        }

        public override string ToString()
        {
            return string.Join(",", Values.Select(a => a.ToString("X")).ToArray());
        }

        uint MurmurHash2(byte[] source)
        {
            uint len = (uint)source.Length;
            uint m = 0x5bd1e995;
            uint seed = 0;
            int r = 24;
            uint h = seed ^ len;

            int offset = 0;
            while (len >= 4)
            {
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

            switch (len)
            {
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
