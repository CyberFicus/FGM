using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericMathUtilities;
using ModularArithmetic;
using System.Numerics;

namespace EllipticCurvesMath.Tests
{
    [TestClass()]
    public class EllipticCurve_BasicFunctionality_GPRTests : PRTestBase.PRTestBase
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
            public static Modulo<T> ModP = new Modulo<T>(_PickSuitablePrime());
            public static EllipticCurve<T> GetCurve()
            {
                var a = GUMU<T>.Random(SharedRandom);
                var b = GUMU<T>.Random(SharedRandom);
                return new EllipticCurve<T>(a, b, ModP);

            }

            public static void GetCoordinatesFromX_AffineResult_GPRTestIteration()
            {
                var curve = GetCurve(); 
                var x = GUMU<T>.Random(curve.P, SharedRandom);
                while (ModP.IsQuadraticResidue(curve.GetRightPart(x)) is false)
                {
                    x = GUMU<T>.Random(curve.P, SharedRandom);
                }

                var coord = curve.GetCoordinatesFromX(x);
                bool act = curve.IsAffine(coord);

                Assert.IsTrue(act, $"\n{coord} does not belong to {curve} for {TypeName}");
            }
            public static void Add_AffineResult_GPRTestIteration()
            {
                var curve = GetCurve();
                var firstPoint = curve.GetRandomPoint(SharedRandom);
                var secondPoint = curve.GetRandomPoint(SharedRandom);

                var thirdPoint = curve.Add(firstPoint, secondPoint).Value;
                var act = curve.IsAffine(thirdPoint);

                Assert.IsTrue(act, $"\n{firstPoint}+{secondPoint}={thirdPoint} does not belong to {curve} for {TypeName}");

                var fourthPoint = curve.Add(firstPoint, firstPoint);
                act = curve.IsAffine(thirdPoint);

                Assert.IsTrue(act, $"\n{firstPoint}+{firstPoint}={fourthPoint} does not belong to {curve} for {TypeName}");
            }
            public static void Multiply_AffineResult_GPRTestIteration()
            {
                var curve = GetCurve();
                var point = curve.GetRandomPoint(SharedRandom);
                var multiplier = SharedRandom.Next();

                var secondPoint = curve.Multiply(multiplier, point);
                var act = curve.IsAffine(secondPoint);

                if (!act)
                    Assert.Fail($"\n{multiplier}*{point}={secondPoint} does not belong to {curve} for {TypeName}");
            }
            public static void Operations_AreConsistent_GPRTestITeration()
            {
                var curve = GetCurve();
                var mulSum = SharedRandom.Next(10);
                var mul1 = SharedRandom.Next(mulSum);
                var mul2 = mulSum - mul1;
                var point = curve.GetRandomPoint(SharedRandom);

                var p1 = curve.Multiply(mul1, point);
                var p2 = curve.Multiply(mul2, point);
                var p3 = curve.Inverse(curve.Multiply(mulSum, point));
                var buf = curve.Add(p1, p2);
                bool act = curve.Add(p3, buf) == null;

                Assert.IsTrue(act, $"{mul1}{point} + {mul2}{point} = {p1} + {p2} = {buf} != {p3} = {mulSum}{point} over {curve} for {TypeName}");
            }
        }
        [TestMethod()]
        public void GetCoordinatesFromX_AffineResult_GPRTest()
        {
            RunIterations(GPRTests<byte>.GetCoordinatesFromX_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ushort>.GetCoordinatesFromX_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<uint>.GetCoordinatesFromX_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ulong>.GetCoordinatesFromX_AffineResult_GPRTestIteration);
        }
        [TestMethod()]
        public void Add_AffineResult_GPRtest()
        {
            RunIterations(GPRTests<byte>.Add_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ushort>.Add_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<uint>.Add_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ulong>.Add_AffineResult_GPRTestIteration);
        }
        [TestMethod()]
        public void Multiply_AffineResult_GPRtest()
        {
            RunIterations(GPRTests<byte>.Multiply_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ushort>.Multiply_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<uint>.Multiply_AffineResult_GPRTestIteration);
            RunIterations(GPRTests<ulong>.Multiply_AffineResult_GPRTestIteration);
        }
        [TestMethod()]
        public void Operations_AreConsistent_GPRTest()
        {
            RunIterations(GPRTests<byte>.Operations_AreConsistent_GPRTestITeration);
            RunIterations(GPRTests<ushort>.Operations_AreConsistent_GPRTestITeration);
            RunIterations(GPRTests<uint>.Operations_AreConsistent_GPRTestITeration);
            RunIterations(GPRTests<ulong>.Operations_AreConsistent_GPRTestITeration);
        }
    }
}