using System;

namespace Ark.Icfp2013 {
    [Flags]
    public enum OperatorType {
        None = 0,
        Shl1 = 1 << 0,
        Shr1 = 1 << 1,
        Shr4 = 1 << 2,
        Shr16 = 1 << 3,
        Not = 1 << 4,
        And = 1 << 5,
        Or = 1 << 6,
        Xor = 1 << 7,
        If0 = 1 << 8,
        Fold = 1 << 9,
        TFold = 1 << 10,
        Bonus = 1 << 11,
        Bonus2 = 1 << 12,
    }
}
