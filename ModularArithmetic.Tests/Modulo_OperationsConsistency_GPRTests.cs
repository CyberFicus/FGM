using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using GenericMathUtilities;

namespace ModularArithmetic.Tests
{
    [TestClass()]
    public class Modulo_OperationsConsistency_GPRTests : PRTestBase.PRTestBase
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
            public static string TypeName = typeof(T).FullName;

            public static Modulo<T> GetMod()
            {
                return new Modulo<T>(GUMU<T>.Random(GUMU<T>.ConvertFrom(2), T.MaxValue, SharedRandom));
            }
            public static T GetVal()
            {
                return GUMU<T>.Random(SharedRandom);
            }

            public static void Add_ConsistentWithSubtract_GPRTestIteration()
            {
                var mod = GetMod();
                var exp1 = GUMU<T>.Random(mod.Modulus, SharedRandom);
                var exp2 = GUMU<T>.Random(mod.Modulus, SharedRandom);
                var sum = mod.Add(exp1, exp2);

                T act1 = mod.Subtract(sum, exp2);
                T act2 = mod.Subtract(sum, exp1);

                Assert.AreEqual(exp1, act1, $"\n {exp1}+{exp2} = {sum} {mod}, but {sum}-{exp2} = {act1} for {TypeName}");
                Assert.AreEqual(exp2, act2, $"\n {exp1}+{exp2} = {sum} {mod}, but {sum}-{exp1} = {act2} for {TypeName}");
            }
            public static void Multiply_ConsistentWithDivide_GPRTestIteration()
            {
                var mod = GetMod();
                var exp1 = GUMU<T>.Random(T.One, mod.Modulus, SharedRandom);
                while (true) 
                {
                    var gcd = GUMU<T>.GetGcd(exp1, mod.Modulus);
                    if (gcd == T.One) break;
                    exp1 /= gcd;
                }
                var exp2 = GUMU<T>.Random(T.One, mod.Modulus, SharedRandom);
                while (true)
                {
                    var gcd = GUMU<T>.GetGcd(exp2, mod.Modulus);
                    if (gcd == T.One) break;
                    exp2 /= gcd;
                }
                T mul = mod.Multiply(exp1, exp2);

                T act1 = mod.Divide(mul, exp2);
                T act2 = mod.Divide(mul, exp1);

                Assert.AreEqual(exp1, act1, $"\n {exp1}*{exp2} = {mul} {mod}, but {mul}/{exp2} = {act1} for {TypeName}");
                Assert.AreEqual(exp2, act2, $"\n {exp1}*{exp2} = {mul} {mod}, but {mul}/{exp1} = {act2} for {TypeName}");
            }
            public static void Power_ConsistentWithNegativePowers_GPRTestITeration()
            {
                var mod = GetMod();
                var val = GUMU<T>.Random(T.One, mod.Modulus - T.One, SharedRandom);
                while (true) 
                {
                    var gcd = GUMU<T>.GetGcd(val, mod.Modulus);
                    if (gcd == T.One) break;
                    val /= gcd;
                }
                int pow = SharedRandom.Next();
                T exp = T.One;

                T act = mod.Multiply(mod.Power(val, pow), mod.Power(val, -pow));

                Assert.AreEqual(exp, act, $"\nTest failed for {val} {mod} as {TypeName}");
            }
        }
        [TestMethod()]
        public void Add_ConsistentWithSubtract_GPRTest()
        {
            RunIterations(GPRTests<byte>.Add_ConsistentWithSubtract_GPRTestIteration);
            RunIterations(GPRTests<ushort>.Add_ConsistentWithSubtract_GPRTestIteration);
            RunIterations(GPRTests<uint>.Add_ConsistentWithSubtract_GPRTestIteration);
            RunIterations(GPRTests<ulong>.Add_ConsistentWithSubtract_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.Add_ConsistentWithSubtract_GPRTestIteration);
        }
        [TestMethod()]
        public void Multiply_ConsistentWithDivide_GPRTest()
        {
            RunIterations(GPRTests<byte>.Multiply_ConsistentWithDivide_GPRTestIteration);
            RunIterations(GPRTests<ushort>.Multiply_ConsistentWithDivide_GPRTestIteration);
            RunIterations(GPRTests<uint>.Multiply_ConsistentWithDivide_GPRTestIteration);
            RunIterations(GPRTests<ulong>.Multiply_ConsistentWithDivide_GPRTestIteration);
            RunIterations(GPRTests<UInt128>.Multiply_ConsistentWithDivide_GPRTestIteration);
        }
        [TestMethod()]
        public void Power_ConsistentWithNegativePowers_GPRTest()
        {
            RunIterations(GPRTests<byte>.Power_ConsistentWithNegativePowers_GPRTestITeration);
            RunIterations(GPRTests<ushort>.Power_ConsistentWithNegativePowers_GPRTestITeration);
            RunIterations(GPRTests<uint>.Power_ConsistentWithNegativePowers_GPRTestITeration);
            RunIterations(GPRTests<ulong>.Power_ConsistentWithNegativePowers_GPRTestITeration);
            RunIterations(GPRTests<UInt128>.Power_ConsistentWithNegativePowers_GPRTestITeration);
        }
    }
}
