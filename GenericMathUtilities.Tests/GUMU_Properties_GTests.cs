using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GenericMathUtilities.Tests
{
    [TestClass()]
    public class GUMU_Properties_GTests 
    {
        public static class GTests<T>
            where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        {
            static string TypeName = typeof(T).Name;

            public static void HalfZerosHalfOnes_CR()
            {
                T expPopCount = T.One << int.Log2(GUMU<T>.Bitlength / 2);
                T expShiftedLeftHalf = T.Zero;

                T actPopCount = T.PopCount(GUMU<T>.HalfZerosHalfOnes);
                T actShiftedLeftHalf = GUMU<T>.HalfZerosHalfOnes >> (GUMU<T>.Bitlength / 2);
                
                Assert.AreEqual(expPopCount, actPopCount, $"\nIncorrect value for {TypeName}");
                Assert.AreEqual(expShiftedLeftHalf, actShiftedLeftHalf, $"\nIncorrect value for {TypeName}");
            }
            public static void SqrtOfOverflow_CR()
            {
                T expValue;
                bool expBool = false;
                {
                    T buf = T.One;
                    while (buf * buf != T.Zero) buf <<= 1;
                    expValue = buf;
                }

                T actValue = GUMU<T>.SqrtOfOverflow;
                bool actBool = ((actValue - T.One) * (actValue - T.One)) == T.Zero;

                Assert.AreEqual(expValue, actValue, $"\nIncorrect value as {TypeName}");
                Assert.AreEqual(expBool, actBool, $"\nWrongful prediction, there is smaller suitable valye as {TypeName}");
            }
        }
        [TestMethod()]
        public void HalfZerosHalfOnes_CR_GTest()
        {
            GTests<byte>.HalfZerosHalfOnes_CR();
            GTests<ushort>.HalfZerosHalfOnes_CR();
            GTests<uint>.HalfZerosHalfOnes_CR();
            GTests<ulong>.HalfZerosHalfOnes_CR();
            GTests<UInt128>.HalfZerosHalfOnes_CR();
        }
        [TestMethod()]
        public void SqrtOfOverflow_CR_Gtest()
        {
            GTests<byte>.SqrtOfOverflow_CR();
            GTests<ushort>.SqrtOfOverflow_CR();
            GTests<uint>.SqrtOfOverflow_CR();
            GTests<ulong>.SqrtOfOverflow_CR();
            GTests<UInt128>.SqrtOfOverflow_CR();
        }
    }
}