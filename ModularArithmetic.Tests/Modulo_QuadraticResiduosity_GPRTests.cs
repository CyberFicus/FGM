using GenericMathUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModularArithmetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularArithmetic.Tests
{
    [TestClass()]
    public class Modulo_QuadraticResiduosity_GPRTests : PRTestBase.PRTestBase
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

        public static class GPRTests<T>
            where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        { 
            static string TypeName = typeof(T).Name;
            
            private static T _PickSuitablePrime()
            {
                ulong[] arr = [7022531, 58889, 251];
                ulong maxVal = GUMU<T>.ConvertTo<ulong>(T.MaxValue);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (maxVal > arr[i])
                        return GUMU<T>.ConvertFrom<ulong>(arr[i]);
                }
                throw new Exception("All prime numbers in array are too big");
            }
            public static Modulo<T> mod = new Modulo<T>(_PickSuitablePrime());

            public static void IsQuadraticResidue_CR_GPRTestIteration()
            {
                var val = GUMU<T>.Random(T.One, mod.Modulus - T.One, SharedRandom);
                var eulersCritertion = false;
                var buf = mod.Power(val, mod.Modulus >> 1);
                if (buf == T.One) eulersCritertion = true;
                var exp = true;

                bool act1 = (eulersCritertion == mod.IsQuadraticResidue(val));
                T valSquared = mod.Multiply(val, val);
                bool act2 = mod.IsQuadraticResidue(valSquared);

                Assert.AreEqual(exp, act1, $"\nInconsistent results for {val} {mod} as {TypeName}");
                Assert.AreEqual(exp, act2, $"\nWrong result for {val}^2 = {valSquared} {mod} as {TypeName}");
            }
            public static void GetSquareRoot_CR_GPRTestIteration()
            {
                var val = GUMU<T>.Random(mod.Modulus, SharedRandom);
                var square = mod.Power(val, 2);
                bool exp = true;    

                var sqrt = mod.GetSqareRoot(square);
                bool act = (val == sqrt || val == mod.Modulus - sqrt);

                Assert.AreEqual(exp, act, $"\nFailed to find square root of {square} {mod} for {TypeName}");
            }
        }
        [TestMethod()]
        public void IsQuadraticResidue_CR_GPRTest()
        {
            RunIterations(GPRTests<byte>.IsQuadraticResidue_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort>.IsQuadraticResidue_CR_GPRTestIteration);
            RunIterations(GPRTests<uint>.IsQuadraticResidue_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong>.IsQuadraticResidue_CR_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.IsQuadraticResidue_CR_GPRTestIteration);
        }
        [TestMethod()]
        public void GetSquareRoot_CR_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetSquareRoot_CR_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetSquareRoot_CR_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetSquareRoot_CR_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetSquareRoot_CR_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetSquareRoot_CR_GPRTestIteration);
        }
    }
}