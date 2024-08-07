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
        public T Modulus { get; private set; }
        public bool CreateImmutableResidues { get; set; }
        
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

        public T Add(T addend1, T addend2)
        {
            var buf = (addend1 + addend2) % Modulus;
            bool bitOverflowOfT = (addend1 > T.MaxValue - addend2);
            if (bitOverflowOfT)
            {
                var overflowModN = (- Modulus) % Modulus; 
                return Add(buf, overflowModN);
            }
            return buf;
        }
        public T Minus(T value)
        {
            if (value > Modulus)
                value %= Modulus;
            return value == T.Zero ? value : Modulus - value;
        }
        public T Subtract(T minuend, T subtrahend)
        {
            return Add(minuend, Minus(subtrahend));
        }  
        public T Multiply(T multiplier1, T multiplier2)
        {
            if (multiplier1 == T.Zero || multiplier2 == T.Zero) { return T.Zero; }
            bool noBitOverflow = (multiplier1 < GUMU<T>.SqrtOfOverflow && multiplier2 < GUMU<T>.SqrtOfOverflow || T.MaxValue / multiplier1 > multiplier2);
            if (noBitOverflow)
            {
                return (multiplier1 * multiplier2) % Modulus;
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
                    result = Add(result, val2ShiftedModN);
                multiplier1 >>= 1;
                val2ShiftedModN = Add(val2ShiftedModN, val2ShiftedModN);
            }
            return result;
        }
        public T Inverse(T value)
        {
            if (value == T.Zero) throw new ArgumentException();
            if (value == T.One) return value;

            T x, y;
            var r = GUMU<T>.GetLinearGcd(value, out x, Modulus, out y);
            if (r > T.One) throw new ModularDivisionException<T>(r, $"Unable to find inverse of {value}; GCD({value},{Modulus}) = {r}");
            x = GUMU<T>.GetModuloAsSignedType(x, Modulus);
            return x;
        }
        public T Divide(T dividend, T divisor)
        {
            return Multiply(dividend, Inverse(divisor));
        }
        public T Power<TBinaryInteger>(T value, TBinaryInteger power)
            where TBinaryInteger : IBinaryInteger<TBinaryInteger>
        {
            if (power < TBinaryInteger.Zero) return Inverse(Power<TBinaryInteger>(value, -power));
            if (power == TBinaryInteger.Zero) return T.One;
            if (power == TBinaryInteger.One) return value % Modulus;

            var result = T.One;
            var valPow2iModN = value;
            for (int i = 1; power > TBinaryInteger.Zero; i++)
            {
                if (TBinaryInteger.IsOddInteger(power))
                {
                    result = Multiply(result, valPow2iModN);
                }
                power >>= 1;
                valPow2iModN = Multiply(valPow2iModN, valPow2iModN); // val^{2^i}
            }
            return result;
        }
        
        private static T _LegendreSymbol(T a, T p)
        {
            if (a == T.Zero || a == T.One) return a;
            T sign;

            if (T.IsEvenInteger(a))
            {
                sign = T.One;
                if ((p % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom(3)) || (p % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom(5)))
                    sign = -T.One;
                return _LegendreSymbol(a >> 1, p) * sign;
            }

            sign = -T.One;
            var aShifted = a >> 1;
            var pShifted = p >> 1;
            if (T.IsEvenInteger(aShifted) || T.IsEvenInteger(pShifted)) sign = T.One;

            return _LegendreSymbol(p % a, a) * sign;
        }
        public bool IsQuadraticResidue(T primeValue)
        {
            return (_LegendreSymbol(primeValue, Modulus) == T.One);
        }
        
        private T _TonneliShanksAlgorithm(T val)
        {
            // Modulus-1 = 2^s * q
            T s = T.Zero; T q = Modulus - T.One;
            while (T.IsEvenInteger(q)) { s++; q >>= 1; }

            T nonQuadraticResidue = T.Zero;
            for (T i = GUMU<T>.ConvertFrom<int>(2); nonQuadraticResidue == T.Zero; i++) if (_LegendreSymbol(i, Modulus) != T.One) nonQuadraticResidue = i;

            T c = Power(nonQuadraticResidue, q);
            T R = Power(val, (q + T.One) >> 1);
            var t = Power(val, q);
            var M = s;

            while (true)
            {
                if (t == T.One) return R;

                // i:  t^2^i = 1 mod p
                T i = T.Zero, i2 = T.One;
                while (Power(t, i2) != T.One && i < M - T.One) { i++; i2 <<= 1; }

                T b = Power(c, Power(GUMU<T>.ConvertFrom<int>(2), M - i - T.One));
                R = Multiply(R, b);
                c = Multiply(b, b);
                t = Multiply(t, c);
                M = i;
            }
        }
        public T GetSqareRoot(T value)
        {
            if (value == T.Zero || value == T.One) return value;

            if (Modulus % GUMU<T>.ConvertFrom<int>(4) == GUMU<T>.ConvertFrom<int>(4))
            {
                return Power(value, (Modulus++)>>2);
            }

            if (Modulus % GUMU<T>.ConvertFrom<int>(8) == GUMU<T>.ConvertFrom<int>(5))
            {
                var m = (Modulus - GUMU<T>.ConvertFrom<int>(5)) >>3;
                T x = Power(value, m + T.One);
                if (Power(x, 2) == value) return x;

                x = Multiply(x, Power(GUMU<T>.ConvertFrom<int>(2),  Add(Multiply(m, GUMU<T>.ConvertFrom<int>(2)),T.One)));
                return x;                
            }

            return _TonneliShanksAlgorithm(value);
        }

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
