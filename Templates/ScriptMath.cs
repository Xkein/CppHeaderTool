

using Scriban.Runtime;

namespace CppHeaderTool.Templates
{
    class ScriptMath : ScriptObject
    {
        public static int BitNot(int a) => ~a;
        public static int BitAnd(int a, int b) => a & b;
        public static int BitOr(int a, int b) => a | b;
        public static int BitXor(int a, int b) => a ^ b;
        public static int BitLshift(int a, int b) => a << b;
        public static int BitRshift(int a, int b) => a >> b;

        public static bool IsAnyBitMasked(int a, int b) => (a & b) != 0;
        public static bool IsAllBitMasked(int a, int b) => (a & b) == b;
    }
}