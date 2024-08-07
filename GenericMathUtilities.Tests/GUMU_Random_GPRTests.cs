using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GenericMathUtilities.Tests
{
    [TestClass()]
    public class GUMU_Random_GPRTests : PRTestBase.PRTestBase
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
            where T: IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        {
            public static string TypeName = typeof(T).Name;

            private class RandomMaxVal : Random
            {
                public override long NextInt64()
                {
                    return long.MaxValue;
                }
            }

            public static void Random_FullBitCoverage()
            {
                T exp = T.MaxValue;

                T act = GUMU<T>.Random(new RandomMaxVal());

                Assert.AreEqual(exp, act, $"\nIncorrect result for {TypeName}");
            } 

            public static void Random_InRange_GPRTestITeration()
            {
                T maxValue = GUMU<T>.Random(SharedRandom);
                T minValue = GUMU<T>.Random(maxValue, SharedRandom);
                bool exp = true;
                
                T rand = GUMU<T>.Random(minValue, maxValue, SharedRandom);
                bool act = (minValue <= rand && rand <= maxValue);

                Assert.AreEqual(exp, act, $"\n{rand} is put of range [{minValue};{maxValue}] for {TypeName}");
            }
        }
        [TestMethod()]
        public void Random_FullBitCoverage_GTest()
        {
            GPRTests<byte>.Random_FullBitCoverage();
            GPRTests<ushort>.Random_FullBitCoverage();
            GPRTests<uint>.Random_FullBitCoverage();
            GPRTests<ulong>.Random_FullBitCoverage();
            GPRTests<UInt128>.Random_FullBitCoverage();
        }
        [TestMethod()]
        public void Random_InRange_GPRTest()
        {
            RunIterations(GPRTests<byte>.Random_InRange_GPRTestITeration);
            RunIterations(GPRTests<ushort>.Random_InRange_GPRTestITeration);
            RunIterations(GPRTests<uint>.Random_InRange_GPRTestITeration);
            RunIterations(GPRTests<ulong>.Random_InRange_GPRTestITeration);
            RunIterations(GPRTests<UInt128>.Random_InRange_GPRTestITeration);
        }
    }
}