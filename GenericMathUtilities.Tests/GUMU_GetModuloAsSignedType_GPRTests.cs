using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GenericMathUtilities.Tests
{
    [TestClass()]
    public class GUMU_GetModuloAsSignedType_GPRTests : PRTestBase.PRTestBase
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
            static string TypeName = typeof(T).Name;

            public static void GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration()
            {
                T mod = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T val = GUMU<T>.Random(GUMU<T>.MsbMask, SharedRandom);
                T exp = val % mod;

                T act = GUMU<T>.GetModuloAsSignedType(val, mod);

                Assert.AreEqual(exp, act, $"\nFailed to convert {val} mod {mod} for {TypeName}");
            }
            public static void GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration()
            {
                T mod = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T val = GUMU<T>.Random(GUMU<T>.MsbMask, SharedRandom);
                T exp = (mod - (val % mod)) % mod;

                T act = GUMU<T>.GetModuloAsSignedType(-val, mod);

                Assert.AreEqual(exp, act, $"\nFailed to convert -{val} mod {mod} for {TypeName}");
            }
        }
        [TestMethod()]
        public void GetModuloAsSignedType_CRForPositiveNumbers_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration);
            RunIterations(GPRTests<ushort>.GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration);
            RunIterations(GPRTests<uint>.GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration);
            RunIterations(GPRTests<ulong>.GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration);
            RunIterations(GPRTests<UInt128>.GetModuloAsSignedType_CRForPositiveNumbers_GPRTestITeration);
        }
        [TestMethod()]
        public void GetModuloAsSignedType_CRForNegativeNumbers_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration);
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration);
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration);
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration);
            RunIterations(GPRTests<byte>.GetModuloAsSignedType_CRForNegativeNumbers_GPRTestITeration);
        }
    }
}