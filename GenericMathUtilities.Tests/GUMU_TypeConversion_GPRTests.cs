using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GenericMathUtilities.Tests
{
    [TestClass()]
    public class GUMU_TypeConversion_GPRTests : PRTestBase.PRTestBase
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            SeedValue = 42;
            PRTestIterations = 1000;
        }
        [TestInitialize]
        public void TestInit()
        {
            ApplySeed();
        }

        public static class GPRTests<T>
            where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        {
            public static string TypeName = typeof(T).Name;

            public static void ConvertFrom_ForLong_CR_GPRTestIteration()
            {
                long val = SharedRandom.NextInt64();
                if (SharedRandom.NextSingle() > 0.5) val = -val;
                ulong exp = (ulong)val;

                T act = GUMU<T>.ConvertFrom<long>(val);

                for (int i = 0; i < GUMU<T>.Bitlength; i++) { 
                    if (ulong.IsEvenInteger(exp) != T.IsEvenInteger(act)) Assert.Fail($"\nBit number {i} is wrong for {val} converted to {TypeName}");
                    exp >>= 1;
                    act >>= 1;
                }
            }
            public static void ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration()
            {
                ulong val = (ulong) SharedRandom.NextInt64();
                if (SharedRandom.NextSingle() > 0.5) val = 0-val;
                if (GUMU<T>.Bitlength < 64)
                    val %= 1ul << GUMU<T>.Bitlength;
                long exp = (long)val;
                T converted = GUMU<T>.ConvertFrom<long>(exp);

                long act = GUMU<T>.ConvertTo<long>(converted);

                Assert.AreEqual(exp, act, $"\nInconsistent conversion reuslts for {TypeName}");
            }
        }
        [TestMethod()]
        public void ConvertFrom_ForLong_CR_GPRTest()
        {
            RunIterations(GPRTests<byte>.ConvertFrom_ForLong_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort>.ConvertFrom_ForLong_CR_GPRTestIteration);
            RunIterations(GPRTests<uint>.ConvertFrom_ForLong_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong>.ConvertFrom_ForLong_CR_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.ConvertFrom_ForLong_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void ConvertTo_ForLong_ReversesConvertFrom_GPRTest()
        {
            RunIterations(GPRTests<byte>.ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration);
            RunIterations(GPRTests<ushort>.ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration);
            RunIterations(GPRTests<uint>.ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration);
            RunIterations(GPRTests<ulong>.ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration);
            RunIterations(GPRTests<UInt128>.ConvertTo_ForLong_ReversesConvertFrom_GPRTestITeration);
        }

    }
}