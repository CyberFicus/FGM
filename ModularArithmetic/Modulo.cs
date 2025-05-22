using System.Numerics;
using GenericMathUtilities;

namespace ModularArithmetic
{
    /// <summary>
    /// Provides arithmetic operations and various utilities for modular residues represented by a type T value 
    /// </summary>
    public class Modulo<T> : IEquatable<Modulo<T>>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        #region properties

        public T Modulus { get; private set; }
        public bool CreateImmutableResidues { get; set; }

        #endregion properties

        public Modulo(T modulus) {
            this.Modulus = modulus; 
            CreateImmutableResidues = true;
        }
        public Residue<T> Residue(T value)
        {
            return new Residue<T>(value, this, CreateImmutableResidues);
        }
        public override string ToString()
        {
            return $"mod {Modulus}";
        }

        #region static math

        public static T Add(T addend1, T addend2, T modulus)
        {
            var buf = (addend1 + addend2) % modulus;
            bool bitOverflowOfT = (addend1 > T.MaxValue - addend2);
            if (bitOverflowOfT)
            {
                var overflowModN = (-modulus) % modulus;
                return Modulo<T>.Add(buf, overflowModN, modulus);
            }
            return buf;
        }

        public static T Minus(T value, T modulus)
        {
            if (value > modulus)
                value %= modulus;
            return value == T.Zero ? value : modulus - value;
        }

        public static T Subtract(T minuend, T subtrahend, T modulus)
        {
            return Modulo<T>.Add(minuend, Modulo<T>.Minus(subtrahend, modulus), modulus);
        }

        public static T Multiply(T multiplier1, T multiplier2, T modulus)
        {
            if (multiplier1 == T.Zero || multiplier2 == T.Zero) { return T.Zero; }
            bool noBitOverflow = (multiplier1 < GUMU<T>.SqrtOfOverflow && multiplier2 < GUMU<T>.SqrtOfOverflow || T.MaxValue / multiplier1 > multiplier2);
            if (noBitOverflow)
            {
                return (multiplier1 * multiplier2) % modulus;
            }

            if (multiplier1 > multiplier2)
            {
                T buf = multiplier2;
                multiplier2 = multiplier1;
                multiplier1 = buf;
            }

            T result = T.Zero;
            T val2ShiftedModN = multiplier2;
            for (int i = 0; multiplier1 > T.Zero; i++)
            {
                if (T.IsOddInteger(multiplier1))
                    result = Modulo<T>.Add(result, val2ShiftedModN, modulus);
                multiplier1 >>= 1;
                val2ShiftedModN = Modulo<T>.Add(val2ShiftedModN, val2ShiftedModN, modulus);
            }
            return result;
        }

        public static T Inverse(T value, T modulus)
        {
            if (value == T.Zero) throw new ArgumentException();
            if (value == T.One) return value;

            T x, y;
            var r = GUMU<T>.GetLinearGcd(value, out x, modulus, out y);
            if (r > T.One) 
                throw new ModularDivisionException<T>(r, $"Unable to find inverse of {value}; GCD({value},{modulus}) = {r}");
            x = GUMU<T>.GetModuloAsSignedType(x, modulus);
            return x;
        }

        public static T Divide(T dividend, T divisor, T modulus)
        {
            return Modulo<T>.Multiply(dividend, Modulo<T>.Inverse(divisor, modulus), modulus);
        }

        public static T Power<TBinaryInteger>(T value, TBinaryInteger power, T modulus)
            where TBinaryInteger : IBinaryInteger<TBinaryInteger>
        {
            if (power < TBinaryInteger.Zero) 
                return Modulo<T>.Inverse(Modulo<T>.Power<TBinaryInteger>(value, -power, modulus), modulus);
            
            if (power == TBinaryInteger.Zero) 
                return T.One;
            
            if (power == TBinaryInteger.One) 
                return value % modulus;

            var result = T.One;
            var valPow2iModN = value;
            for (int i = 1; power > TBinaryInteger.Zero; i++)
            {
                if (TBinaryInteger.IsOddInteger(power))
                {
                    result = Modulo<T>.Multiply(result, valPow2iModN, modulus);
                }
                power >>= 1;
                valPow2iModN = Modulo<T>.Multiply(valPow2iModN, valPow2iModN, modulus); // val^{2^i}
            }
            return result;
        }

        public static T LegendreSymbol(T a, T p)
        {
            if (a == T.Zero || a == T.One) return a;

            T sign;

            if (T.IsEvenInteger(a))
            {
                sign = T.One;
                if ((p % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom(3)) || (p % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom(5)))
                    sign = -T.One;
                return LegendreSymbol(a >> 1, p) * sign;
            }

            sign = -T.One;
            var aShifted = a >> 1;
            var pShifted = p >> 1;
            if (T.IsEvenInteger(aShifted) || T.IsEvenInteger(pShifted)) sign = T.One;

            return LegendreSymbol(p % a, a) * sign;
        }

        public static bool IsQuadraticResidue(T primeValue, T modulus)
        {
            return LegendreSymbol(primeValue, modulus) == T.One;
        }

        private static T TonneliShanksAlgorithm(T value, T modulus)
        {
            // Modulus-1 = 2^s * q
            T s = T.Zero; T q = modulus - T.One;
            while (T.IsEvenInteger(q)) { s++; q >>= 1; }

            T nonQuadraticResidue = T.Zero;
            for (T i = GUMU<T>.ConvertFrom<int>(2); nonQuadraticResidue == T.Zero; i++) if (LegendreSymbol(i, modulus) != T.One) nonQuadraticResidue = i;

            T c = Modulo<T>.Power(nonQuadraticResidue, q, modulus);
            T R = Modulo<T>.Power(value, (q + T.One) >> 1, modulus);
            var t = Modulo<T>.Power(value, q, modulus);
            var M = s;

            while (true)
            {
                if (t == T.One) return R;

                // i:  t^2^i = 1 mod p
                T i = T.Zero, i2 = T.One;
                while (Modulo<T>.Power(t, i2, modulus) != T.One && i < M - T.One) { i++; i2 <<= 1; }

                T b = Power(c, Modulo<T>.Power(GUMU<T>.ConvertFrom<int>(2), M - i - T.One, modulus), modulus);
                R = Modulo<T>.Multiply(R, b, modulus);
                c = Modulo<T>.Multiply(b, b, modulus);
                t = Modulo<T>.Multiply(t, c, modulus);
                M = i;
            }
        }

        public static T GetSqareRoot(T value, T modulus)
        {
            if (value == T.Zero || value == T.One) return value;

            if (modulus % GUMU<T>.ConvertFrom<int>(4) == GUMU<T>.ConvertFrom<int>(4))
            {
                return Modulo<T>.Power(value, (modulus++) >> 2, modulus);
            }

            if (modulus % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom<int>(5))
            {
                var m = (modulus - GUMU<T>.ConvertFrom<int>(5)) >> 3;
                T x = Modulo<T>.Power(value, m + T.One, modulus);
                if (Modulo<T>.Power(x, 2, modulus) == value) return x;

                x = Modulo<T>.Multiply(x, Power(GUMU<T>.ConvertFrom<int>(2), Add(Modulo<T>.Multiply(m, GUMU<T>.ConvertFrom<int>(2), modulus), T.One, modulus),modulus), modulus);
                return x;
            }

            return Modulo<T>.TonneliShanksAlgorithm(value, modulus);
        }

        #endregion static math

        #region non-static math

        public T Add(T addend1, T addend2)
        {
            return Modulo<T>.Add(addend1, addend2, Modulus);
        }
 
        public T Minus(T value)
        {
            return Modulo<T>.Minus(value, Modulus);
        }

        public T Subtract(T minuend, T subtrahend)
        {
            return Modulo<T>.Subtract(minuend, subtrahend, Modulus);
        }
        
        public T Multiply(T multiplier1, T multiplier2)
        {
            return Modulo<T>.Multiply(multiplier1, multiplier2, Modulus);
        }
        
        public T Inverse(T value)
        {
            return Modulo<T>.Inverse(value, Modulus);
        }

        public T Divide(T dividend, T divisor)
        {
            return Modulo<T>.Divide(dividend, divisor, Modulus);
        }

        public T Power<TBinaryInteger>(T value, TBinaryInteger power)
            where TBinaryInteger : IBinaryInteger<TBinaryInteger>
        {
            return Modulo<T>.Power<TBinaryInteger>(value, power, Modulus);
        }
        
        public bool IsQuadraticResidue(T primeValue)
        {
            return Modulo<T>.IsQuadraticResidue(primeValue, Modulus);
        }   

        public T GetSqareRoot(T value)
        {
            return Modulo<T>.GetSqareRoot(value, Modulus);
        }

        #endregion non-static math

        public override int GetHashCode()
        {
            return HashCode.Combine(Modulus, GUMU<T>.Bitlength);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Modulo<T>) return false;
            return Equals((Modulo<T>)obj);
        }
        public bool Equals(Modulo<T>? other)
        {
            if (other == null) return false;
            if (Modulus != other.Modulus) return false;
            return true;
        }
        public static bool operator ==(Modulo<T>? left, Modulo<T>? right)
        {
            if (left is null)
            {
                if (right is null) return true;
                return false;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Modulo<T>? left, Modulo<T>? right)
        {
            return !(left == right);
        }
    }
}
