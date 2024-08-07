using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GenericMathUtilities.Tests
{
    [TestClass()]
    public class GUMU_GcdMethods_GPRTests : PRTestBase.PRTestBase
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

            /// <summary>
            /// Generates two coprime numbers. Each of them, when multiplied by commonMultiplier, does not overflow type T
            /// </summary>
            public static void GenerateSuitableCoprimes(T commonMultiplier, out T coprime1, out T coprime2)
            {
                T val1 = GUMU<T>.Random(T.One, T.MaxValue / commonMultiplier, SharedRandom);
                T val2 = GUMU<T>.Random(T.One, T.MaxValue / commonMultiplier, SharedRandom);
                T gcd = GUMU<T>.GetGcd(val1, val2);
                coprime1 = val1 / gcd;
                coprime2 = val2 / gcd;
            }

            public static void GetGcd_DetectsSmallGcd()
            {
                T lastNumber = GUMU<T>.ConvertFrom<byte>(byte.MaxValue);
                if (lastNumber > GUMU<T>.HalfZerosHalfOnes)
                    lastNumber = GUMU<T>.HalfZerosHalfOnes;
                for (T i = T.One; i <= lastNumber; i++)
                {
                    T exp = i, coprime1, coprime2;
                    GenerateSuitableCoprimes(exp, out coprime1, out coprime2);
                    T a = coprime1 * exp, b = coprime2 * exp;

                    T act = GUMU<T>.GetGcd(a, b);

                    Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {a} and {b} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
                }
            }
            public static void GetLinearGcd_DetectsSmallGcd()
            {
                T lastNumber = GUMU<T>.ConvertFrom<byte>(byte.MaxValue);
                if (lastNumber > GUMU<T>.HalfZerosHalfOnes)
                    lastNumber = GUMU<T>.HalfZerosHalfOnes;
                for (T i = T.One; i <= lastNumber; i++)
                {
                    T exp = i, coprime1, coprime2;
                    GenerateSuitableCoprimes(exp, out coprime1, out coprime2);
                    T a = coprime1 * exp, b = coprime2 * exp;

                    T act = GUMU<T>.GetLinearGcd(a, out T x, b, out T y);

                    Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {a} and {b} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
                }
            }
            public static void GetGcd_DetectsCoprimes_GPRTestIteration()
            {
                T coprime1, coprime2;
                T exp = T.One;
                GenerateSuitableCoprimes(exp, out coprime1, out coprime2);

                T act = GUMU<T>.GetGcd(coprime1, coprime2);

                Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {coprime1} and {coprime2} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
            }
            public static void GetLinearGcd_DetectsCoprimes_GPRTestIteration()
            {
                T coprime1, coprime2;
                T exp = T.One;
                GenerateSuitableCoprimes(exp, out coprime1, out coprime2);

                T act = GUMU<T>.GetLinearGcd(coprime1, out T x, coprime2, out T y);

                Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {coprime1} and {coprime2} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
            }
            public static void GetGcd_DetectsNonCoprimes_GPRTestIteration()
            {
                T exp = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T coprime1, coprime2;
                GenerateSuitableCoprimes(exp, out coprime1, out coprime2);
                T a = coprime1 * exp, b = coprime2 * exp;

                T act = GUMU<T>.GetGcd(a, b);

                Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {a} and {b} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
            }
            public static void GetLinearGcd_DetectsNonCoprimes_GPRTestIteration()
            {
                T exp = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T coprime1, coprime2;
                GenerateSuitableCoprimes(exp, out coprime1, out coprime2);
                T a = coprime1 * exp, b = coprime2 * exp;

                T act = GUMU<T>.GetLinearGcd(a, out T x, b, out T y);

                Assert.AreEqual(exp, act, $"\nIncorrect GCD of numbers {a} and {b} for {TypeName}\nIteration used supposedly coprime numbers {coprime1} and {coprime2}");
            }
            public static void GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration()
            {
                T value1 = GUMU<T>.Random(SharedRandom);
                T value2 = GUMU<T>.Random(SharedRandom);
                bool excGetGcd = false;
                bool excGetLinearGcd = false;
                bool exp = true;

                T gcd1 = T.Zero;
                try
                {
                    gcd1 = GUMU<T>.GetGcd(value1, value2);
                }
                catch (ArgumentException)
                {
                    if (value1 == T.Zero || value2 == T.Zero)
                        excGetGcd = true;
                    else
                        Assert.Fail($"\nGetGcd wrongfully recognized one of the values {value1} and {value2} for type {TypeName}");
                }
                T gcd2 = T.One;
                try
                {
                    gcd2 = GUMU<T>.GetLinearGcd(value1, out T x, value2, out T y);
                }
                catch (ArgumentException)
                {
                    if (value1 == T.Zero || value2 == T.Zero)
                        excGetLinearGcd = true;
                    else
                        Assert.Fail($"\nGetLinearGcd wrongfully recognized one of the values {value1} and {value2} as {TypeName}");
                }
                bool actExceptionConsistency = (excGetGcd == excGetLinearGcd);
                bool actResultConsistency = (gcd1 == gcd2);

                Assert.AreEqual(exp, actExceptionConsistency, $"\nInconsistent zero argument exception throwing for values {value1} and {value2} as {TypeName}");
                if (excGetGcd == false)
                    Assert.AreEqual(exp, actResultConsistency, $"\nGetGcd returned {gcd1} and GetLinearGcd returned {gcd2} for values {value1} and {value2} as {TypeName}");
            }
            public static void GetLinearGcd_CorrectCoefficients_GPRTestIteration()
            {
                T val1 = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T val2 = GUMU<T>.Random(T.One, T.MaxValue, SharedRandom);
                T exp = GUMU<T>.GetGcd(val1, val2);
                T x, y;
                GUMU<T>.GetLinearGcd(val1, out x, val2, out y);

                T act = val1 * x + val2 * y;

                Assert.AreEqual(exp, act, $"\nLinear coefficients {x} and {y} do not represent {exp} correctly for {TypeName}");
            }
        }
        [TestMethod()]
        public void GetGcd_DetectsSmallCoprimes_GTest()
        {
            GPRTests<byte>.GetGcd_DetectsSmallGcd();
            GPRTests<ushort>.GetGcd_DetectsSmallGcd();
            GPRTests<ulong>.GetGcd_DetectsSmallGcd();
            GPRTests<uint>.GetGcd_DetectsSmallGcd();
            GPRTests<UInt128>.GetGcd_DetectsSmallGcd();
        }
        [TestMethod()]
        public void GetLinearGcd_DetectsSmallCoprimes_GTest()
        {
            GPRTests<byte>.GetLinearGcd_DetectsSmallGcd();
            GPRTests<ushort>.GetLinearGcd_DetectsSmallGcd();
            GPRTests<ulong>.GetLinearGcd_DetectsSmallGcd();
            GPRTests<uint>.GetLinearGcd_DetectsSmallGcd();
            GPRTests<UInt128>.GetLinearGcd_DetectsSmallGcd();
        }
        [TestMethod()]
        public void GetGcd_DetectsCoprimes_GPRTes()
        {
            RunIterations(GPRTests<byte>.GetGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetGcd_DetectsCoprimes_GPRTestIteration);
        }
        [TestMethod()]
        public void GetLinearGcd_DetectsCoprimes_GPRTes()
        {
            RunIterations(GPRTests<byte>.GetLinearGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetLinearGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetLinearGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetLinearGcd_DetectsCoprimes_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetLinearGcd_DetectsCoprimes_GPRTestIteration);
        }
        [TestMethod()]
        public void GetGcd_DetectsNonCoprimes_GPRTes()
        {
            RunIterations(GPRTests<byte>.GetGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetGcd_DetectsNonCoprimes_GPRTestIteration);
        }
        [TestMethod()]
        public void GetLinearGcd_DetectsNonCoprimes_GPRTes()
        {
            RunIterations(GPRTests<byte>.GetLinearGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetLinearGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetLinearGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetLinearGcd_DetectsNonCoprimes_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetLinearGcd_DetectsNonCoprimes_GPRTestIteration);
        }
        [TestMethod()]
        public void GetLinearGcd_ConsistentResultsAndExceptions_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetLinearGcd_ConsistentResultsAndExceptions_GPRTestIteration);
        }
        [TestMethod()]
        public void GetLinearGcd_CorrectCoefficients_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetLinearGcd_CorrectCoefficients_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetLinearGcd_CorrectCoefficients_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetLinearGcd_CorrectCoefficients_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetLinearGcd_CorrectCoefficients_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.GetLinearGcd_CorrectCoefficients_GPRTestIteration);
        }
    }
}