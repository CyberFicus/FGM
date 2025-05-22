using System.Numerics;

namespace GenericMathUtilities
{
    /// <summary>
    /// Generic Unsigned Math Utilities for binary unsigned integer types which have minimal and maximal values defined
    /// </summary>
    public static class GUMU<T>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        private static T _msbMask = AllOnes ^ (AllOnes >> 1);
        private static int _bitLength = T.Zero.GetByteCount() * 8;
        private static T _halfZerosHalfOnes = AllOnes >> (_bitLength / 2);
        private static T _sqrtOfOverflow = _halfZerosHalfOnes + T.One;
        /// <summary>
        /// Type T value with all bytes set to 0xff
        /// </summary>
        public static T AllOnes { get { return T.MaxValue; } }
        /// <summary>
        /// Type T value with the most significant bit set to 1 and all other bits set to 0
        /// </summary>
        public static T MsbMask { get { return _msbMask; } }
        /// <summary>
        /// Number of bits stored by type T
        /// </summary>
        public static int Bitlength { get { return _bitLength; } }
        /// <summary>
        /// Type T value where all bits in left half are zeroed out and all bits in the right part are set to 1.
        /// <para>E.g. 0x0f for Byte, 0x00ff for UInt16, etc</para>
        /// </summary>
        public static T HalfZerosHalfOnes { get { return _halfZerosHalfOnes; } }
        /// <summary>
        /// If both A and B are less that this value, multiplication of A*B wont overflow type T 
        /// </summary>
        public static T SqrtOfOverflow { get { return _sqrtOfOverflow; } }
        /// <summary>
        /// Returns true if most significant bit is set to 1. Otherwise returns false
        /// </summary>
        public static bool GetMsb(T value)
        {
            return ((value & MsbMask) != T.Zero);
        }

        /// <summary>
        /// Converts value from type TOther to type T.
        /// <para>If some bits dont fit, they are dropped. E.g. UInt16 0x0123 will be converted to Byte 0x23 </para>
        /// </summary>
        public static T ConvertFrom<TOther>(TOther value)
            where TOther : IBinaryInteger<TOther>, IMinMaxValue<TOther>
        {
            TOther bitDetectorO = TOther.One;
            T bitAssignerT = T.One;
            int bitBorder = int.Min(Bitlength, TOther.Zero.GetByteCount() * 8);
            T res = T.Zero;
            for (int i = 0; i <= bitBorder; i++)
            {
                if ((value & bitDetectorO) != TOther.Zero)
                {
                    res = res | bitAssignerT;
                    value -= bitDetectorO;
                    if (value == TOther.Zero) return res;
                }
                bitDetectorO <<= 1;
                bitAssignerT <<= 1;
            }
            return res;
        }
        /// <summary>
        /// Converts value from type T to type TOther. If some bits dont fit, they are dropped
        /// <para>If some bits dont fit, they are dropped. E.g. UInt16 0x0123 will be converted to Byte 0x23 </para>
        /// </summary>
        public static TOther ConvertTo<TOther>(T value)
            where TOther : IBinaryInteger<TOther>, IMinMaxValue<TOther>
        {
            T bitDetectorT = T.One;
            TOther bitAssignerO = TOther.One;
            int bitBorder = int.Min(Bitlength, TOther.Zero.GetByteCount() * 8);
            var res = TOther.Zero;
            for (int i = 0; i <= bitBorder; i++)
            {
                if ((value & bitDetectorT) != T.Zero)
                {
                    res = res | bitAssignerO;
                    value -= bitDetectorT;
                    if (value == T.Zero) return res;
                }
                bitDetectorT <<= 1;
                bitAssignerO <<= 1;
            }
            return res;
        }

        /// <summary>
        /// Returns a random value of type T
        /// </summary>
        /// <param name="random">Uses passed instance of Random or creates new one if the passed one is null</param>
        public static T Random(Random? random = null)
        {
            if (random == null) random = new Random();
            if (Bitlength <= 63) ConvertFrom<long>(random.NextInt64());

            T res = T.Zero;
            int bitsSet = 0;
            while (bitsSet < Bitlength)
            {
                res += (ConvertFrom<long>(random.NextInt64()) << bitsSet);
                bitsSet += 63;
            }
            return res;
        }
        /// <summary>
        /// Returns a random value of type T that is less then a specified maximum
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound of the random number to be generated</param>
        /// <param name="random">Uses passed instance of Random or creates new one if the passed one is null</param>
        public static T Random(T maxValue, Random? random = null)
        {
            if (maxValue == T.Zero) return T.Zero;
            return Random(random) % maxValue;
        }
        /// <summary>
        /// Returns a random value of type T in specified range
        /// </summary>
        /// <param name="minValue">Inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">Exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <param name="random">Uses passed instance of Random or creates new one if the passed one is null</param>
        public static T Random(T minValue, T maxValue, Random? random = null)
        {
            if (maxValue < minValue) throw new ArgumentOutOfRangeException("maxValue is smaller then minValue");
            if (minValue == T.Zero)
            {
                if (maxValue == T.MaxValue) return Random(random);
                return Random(maxValue + T.One, random);
            }
            T rangeLength = maxValue - minValue + T.One;
            return minValue + Random(rangeLength, random);
        }

        /// <summary>
        /// Calculates GCD (greatest common divisor) of two values
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static T GetGcd(T firstValue, T secondValue)
        {
            if (firstValue == T.Zero || secondValue == T.Zero) throw new ArgumentException($"GCD: one of the values {firstValue}, {secondValue} is zero");

            T previous, current, next;
            if (firstValue > secondValue) { previous = firstValue; current = secondValue; }
            else { previous = secondValue; current = firstValue; }

            while (true)
            {
                next = previous % current;
                if (next == T.Zero) break;
                previous = current;
                current = next;
            }

            return current;
        }
        /// <summary>
        /// Calculates GCD (greatest common divisor) of two values and coefficients of its linear form as well
        /// <para>Linear form: firstValue * firstCoefficient + secondValue * secondCoefficient = gcd</para>
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static T GetLinearGcd(T firstValue, out T firstCoefficient, T secondValue, out T secondCoefficient)
        {
            if (firstValue == T.Zero || secondValue == T.Zero) throw new ArgumentException($"LinearGCD: one of the values {firstValue}, {secondValue} is zero");

            T previous, previousFirstMultiplier, previousSecondMultiplier;
            T current, currentFirstMultiplier, currentSecondMultiplier;
            T next, nextFirstMultiplier, nextSecondMultiplier;
            T q;

            if (firstValue > secondValue)
            {
                previous = firstValue; previousFirstMultiplier = T.One; previousSecondMultiplier = T.Zero;
                current = secondValue; currentFirstMultiplier = T.Zero; currentSecondMultiplier = T.One;
            }
            else
            {
                previous = secondValue; previousFirstMultiplier = T.Zero; previousSecondMultiplier = T.One;
                current = firstValue; currentFirstMultiplier = T.One; currentSecondMultiplier = T.Zero;
            }

            while (true)
            {
                next = previous % current;
                q = (previous - next) / current;

                if (next == T.Zero) { break; }

                nextFirstMultiplier = previousFirstMultiplier - (currentFirstMultiplier * q);
                nextSecondMultiplier = previousSecondMultiplier - (currentSecondMultiplier * q);

                previous = current; previousFirstMultiplier = currentFirstMultiplier; previousSecondMultiplier = currentSecondMultiplier;
                current = next; currentFirstMultiplier = nextFirstMultiplier; currentSecondMultiplier = nextSecondMultiplier;
            }
            firstCoefficient = currentFirstMultiplier;
            secondCoefficient = currentSecondMultiplier;

            return current;
        }
        /// <summary>
        /// Given value and modulus, calculates residue. Treats value as a negative value of signed type if its most significant bit is set to 1
        /// <para>Designed specifically to work with output of GUMU.GetLinearGcd</para>
        /// </summary>
        public static T GetModuloAsSignedType(T value, T modulus)
        {
            var treatAsNegative = GetMsb(value);
            if (treatAsNegative)
            {
                T removedMinus = -value;
                T takenbyMod = removedMinus % modulus;
                if (takenbyMod != T.Zero)
                    return modulus - takenbyMod;
                return takenbyMod;
            }
            return value % modulus;
        }
    }
}
