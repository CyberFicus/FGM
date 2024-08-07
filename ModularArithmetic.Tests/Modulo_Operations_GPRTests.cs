using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using GenericMathUtilities;

namespace ModularArithmetic.Tests
{
    [TestClass()]
    public class Modulo_Operations_GPRTests : PRTestBase.PRTestBase
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            SeedValue = 42;
            PRTestIterations = 1000;
        }
        [TestInitialize()]
        public void TestInit()
        {
            ApplySeed();
        }

        public static class GPRTests<T, TOverflowSafe>
            where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
            where TOverflowSafe : IBinaryInteger<TOverflowSafe>, IUnsignedNumber<TOverflowSafe>, IMinMaxValue<TOverflowSafe>
        {
            public static string TypeName = typeof(T).FullName;

            public static Modulo<T> GetMod()
            {
                return new Modulo<T>(GUMU<T>.Random(GUMU<T>.ConvertFrom(2), T.MaxValue, SharedRandom));
            }
            public static T GetVal()
            {
                return GUMU<T>.Random(SharedRandom);
            }

            public static void Add_CR_GPRTestIteration()
            {
                var mod = GetMod();
                T val1 = GetVal();
                T val2 = GetVal();
                TOverflowSafe buf = ((GUMU<TOverflowSafe>.ConvertFrom<T>(val1)+ GUMU<TOverflowSafe>.ConvertFrom<T>(val2))% GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus));
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>(buf);

                T act = mod.Add(val1, val2);

                Assert.AreEqual(exp, act, $"\nFailed to calculate {val1} + {val2} {mod} for {TypeName}");
            }
            public static void Minus_CR_GPRTestIteration()
            {
                var mod = GetMod();
                T val = GetVal();
                TOverflowSafe buf = GUMU<TOverflowSafe>.GetModuloAsSignedType(-GUMU<TOverflowSafe>.ConvertFrom<T>(val), GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus));
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>(buf);

                var act = mod.Minus(val);

                Assert.AreEqual(exp, act, $"\nFailed to calculate -({val}) {mod} for {TypeName}");
            }
            public static void Subtract_CR_GPRTestIteration()
            {
                var mod = GetMod();
                T val1 = GetVal();
                T val2 = GetVal();
                TOverflowSafe buf = GUMU<TOverflowSafe>.GetModuloAsSignedType(GUMU<TOverflowSafe>.ConvertFrom<T>(val1) - GUMU<TOverflowSafe>.ConvertFrom<T>(val2), GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus));
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>(buf);

                T act = mod.Subtract(val1, val2);

                Assert.AreEqual(exp, act, $"\nFailed to calculate {val1} - {val2} {mod} for {TypeName}");
            }
            public static void Multiply_CR_GPRTestIteration()
            {
                var mod = GetMod();
                var val1 = GetVal();
                var val2 = GetVal();
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>((GUMU<TOverflowSafe>.ConvertFrom<T>(val1) * GUMU<TOverflowSafe>.ConvertFrom<T>(val2)) % GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus));

                var act = mod.Multiply(val1, val2);

                Assert.AreEqual(exp, act, $"\nFailed to calculate {val1} * {val2} {mod} for {TypeName}");
            }
            public static void Inverse_CR_GPRTestIteration()
            {
                var mod = GetMod();
                var val = GetVal();
                TOverflowSafe exp = TOverflowSafe.One;
                T act = T.Zero;
                bool excFlag = false;
                try
                {
                    act = mod.Inverse(val);
                }
                catch (ArgumentException e)
                {
                    excFlag = true;
                    if (val != T.Zero)
                        Assert.Fail($"\nUnable to didvide by {val}");
                }
                catch (ModularDivisionException<T> e)
                {
                    excFlag = true;
                    var divisor = e.CommonDivisor;
                    var gcd = GUMU<T>.GetGcd(mod.Modulus, val);
                    if (divisor != gcd || gcd == T.One)
                    {
                        Assert.Fail($"Wrongly detected divisor {divisor} while inversing {val} {mod}. Actual GCD is {gcd}");
                    }
                }
                if (!excFlag)
                    Assert.AreEqual(exp, (GUMU<TOverflowSafe>.ConvertFrom<T>(val) * GUMU<TOverflowSafe>.ConvertFrom<T>(act)) % GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus), $"\nFailed to calculate {val}^{{-1}} {mod}");

            }
            public static void Power_ForInt_SmallPowersCR_GPRTestIteration()
            {
                var mod = GetMod();
                var val = GetVal();
                int pow = SharedRandom.Next(65);
                TOverflowSafe buf = TOverflowSafe.One, val1 = GUMU<TOverflowSafe>.ConvertFrom<T>(val), m = GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus);
                for (int i = 0; i < pow; i++)
                {
                    buf = (buf * val1) % m;
                }
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>(buf);

                T act = mod.Power<int>(val, pow);

                Assert.AreEqual(exp, act, $"\nFailed to calculate {val}^{pow}{mod} as {TypeName}");
            }
            public static void Power_ForInt_CR_GPRTestIteration()
            {
                static TOverflowSafe TestPower(TOverflowSafe val, int pow, TOverflowSafe mod)
                {
                    if (pow == 0) return TOverflowSafe.One;
                    TOverflowSafe res = TOverflowSafe.One;
                    if (pow % 2 == 1)
                        res *= val;
                    return (res * TestPower((val*val) % mod, pow>>1, mod)) % mod;
                }

                var mod = GetMod();
                var val = GetVal();
                int pow = SharedRandom.Next();
                TOverflowSafe buf = TestPower(GUMU<TOverflowSafe>.ConvertFrom<T>(val), pow, GUMU<TOverflowSafe>.ConvertFrom<T>(mod.Modulus));
                T exp = GUMU<T>.ConvertFrom<TOverflowSafe>(buf);

                T act = mod.Power<int>(val, pow);

                Assert.AreEqual(exp, act, $"\nFailed to calculate {val}^{pow}{mod} as {TypeName}");
            }
        }
        [TestMethod()]
        public void Add_CR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Add_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Add_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Add_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Add_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void Minus_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Minus_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Minus_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Minus_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Minus_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void Subtract_CR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Subtract_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Subtract_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Subtract_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Subtract_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void Multiply_CR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Multiply_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Multiply_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Multiply_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Multiply_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void Inverse_CR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Inverse_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Inverse_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Inverse_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Inverse_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void Power_ForInt_SmallPowersCR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Power_ForInt_SmallPowersCR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Power_ForInt_SmallPowersCR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Power_ForInt_SmallPowersCR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Power_ForInt_SmallPowersCR_GPRTestIteration);
        }
        [TestMethod()]
        public void Power_ForInt_CR_GPRTest()
        {
            RunIterations(GPRTests<byte, ulong>.Power_ForInt_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort, ulong>.Power_ForInt_CR_GPRTestIteration);
            RunIterations(GPRTests<uint, ulong>.Power_ForInt_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong, UInt128>.Power_ForInt_CR_GPRTestIteration);
        }
    }
}