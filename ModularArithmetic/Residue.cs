using System.Numerics;

namespace ModularArithmetic
{
    /// <summary>
    /// Wrapper class for convenient usage of Modulo class
    /// <para>Wraps modular residue represented by type T and an instance of Modulo</para>
    /// </summary>
    public class Residue<T> : IEquatable<Residue<T>>
            where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public Modulo<T> Modulo { get; private set; }
        public T Modulus { get { return Modulo.Modulus; } }
        public T Value { get; private set; }
        /// <summary>
        /// Defines whether methods will change this instance or create new ones
        /// </summary>
        public bool IsImmutable { get; set; }

        public Residue(T value, Modulo<T> modulo, bool IsImmutable = true)
        {
            Modulo = modulo;
            Value = value % Modulo.Modulus;
            this.IsImmutable = IsImmutable;
        }
        public Residue(Residue<T> sourceResidue, bool? IsImmutable = null)
        {
            Modulo = sourceResidue.Modulo;
            Value = sourceResidue.Value;
            if (IsImmutable == null)
            {
                this.IsImmutable = sourceResidue.IsImmutable;
            } else
            {
                this.IsImmutable = IsImmutable.Value;
            }
        }

        private static void _ThrowExcIfModulusMismatch(Residue<T> first, Residue<T> second)
        {
            if (first.Modulo.Modulus != second.Modulo.Modulus) throw new ArgumentException($"Modulus of {first} and {second} mismatch");
        }
        public override string ToString()
        {
            return $"{Value} mod {Modulo.Modulus}";
        }

        public Residue<T> ChangeValue(T newValue)
        {
            if (IsImmutable)
            {
                return new Residue<T>(newValue, this.Modulo);
            }
            Value = newValue % Modulus;
            return this;
        }
        public Residue<T> ChangeModulus(Modulo<T> newModulo)
        {
            if (IsImmutable)
            {
                return new Residue<T>(this.Value, newModulo);
            }
            Modulo = newModulo;
            Value %= Modulus;
            return this;
        }
        public Residue<T> ChangeModulus(T newModulus)
        {
            var newModulo = new Modulo<T>(newModulus); 
            if (IsImmutable)
            {
                return new Residue<T>(this.Value, newModulo);
            }
            Modulo = newModulo;
            Value %= Modulus;
            return this;
        }

        public static bool operator ==(Residue<T> left, Residue<T> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if ((left.Modulo != right.Modulo)) return false;
            if (left.Value == right.Value) return true;
            return false;
        }
        public static bool operator !=(Residue<T> left, Residue<T> right)
        {
            return !(left == right);
        }
        public static bool operator ==(Residue<T> left, T right)
        {
            return left.Value == right % left.Modulus;
        }
        public static bool operator !=(Residue<T> left, T right)
        {
            return !(left == right);
        }
        public static bool operator ==(T left, Residue<T> right)
        {
            return right == left;
        }
        public static bool operator !=(T left, Residue<T> right)
        {
            return !(left == right);
        }

        public static Residue<T> operator +(Residue<T> left, T right)
        {
            var res = left.Modulo.Add(left.Value, right);
            if (left.IsImmutable)
            {
                return new Residue<T>(res, left.Modulo);
            }
            left.Value = res;
            return left;
        }
        public static Residue<T> operator +(Residue<T> left, Residue<T> right)
        {
            _ThrowExcIfModulusMismatch(left, right);
            return left + right.Value;
        }
        public static Residue<T> operator +(T left, Residue<T> right)
        {
            return right + left;
        }
        public static Residue<T> operator ++(Residue<T> residue)
        {
            return residue + T.One;
        }

        public static Residue<T> operator -(Residue<T> left, T right)
        {
            var res = left.Modulo.Subtract(left.Value, right);
            if (left.IsImmutable)
            {
                return new Residue<T>(res, left.Modulo);
            }
            left.Value = res;
            return left;
        }
        public static Residue<T> operator -(Residue<T> left, Residue<T> right)
        {
            _ThrowExcIfModulusMismatch(left, right);
            return left - right.Value;
        }
        public static Residue<T> operator -(T left, Residue<T> right)
        {
            var res = right.Modulo.Subtract(left, right.Value);
            if (right.IsImmutable)
            {
                return new Residue<T>(res, right.Modulo);
            }
            right.Value = res;
            return right;
        }
        public static Residue<T> operator --(Residue<T> residue)
        {
            return residue - T.One;
        }
        public static Residue<T> operator -(Residue<T> residue)
        {
            return T.Zero - residue;
        }

        public static Residue<T> operator *(Residue<T> left, T right)
        {
            var res = left.Modulo.Multiply(left.Value, right);
            if (left.IsImmutable)
            {
                return new Residue<T>(res, left.Modulo);
            }
            left.Value = res;
            return left;
        }
        public static Residue<T> operator *(Residue<T> left, Residue<T> right)
        {
            _ThrowExcIfModulusMismatch(left, right);
            return left * right.Value;
        }
        public static Residue<T> operator *(T left, Residue<T> right)
        {
            return right * left;
        }

        public Residue<T> Inverse()
        {
            var res = Modulo.Inverse(Value);
            if (IsImmutable)
            {
                return new Residue<T>(res, Modulo);
            }
            Value = res; 
            return this;
        }
        public static Residue<T> operator /(Residue<T> left, T right)
        {
            var res = left.Modulo.Divide(left.Value, right);
            if (left.IsImmutable)
            {
                return new Residue<T>(res, left.Modulo);
            }
            left.Value = right;
            return left;
        }
        public static Residue<T> operator /(Residue<T> left, Residue<T> right)
        {
            _ThrowExcIfModulusMismatch(left, right);
            return left / right.Value;
        }
        public static Residue<T> operator /(T left, Residue<T> right)
        {
            var res = right.Modulo.Divide(left, right.Value);
            if (right.IsImmutable)
            {
                return new Residue<T>(res, right.Modulo);
            }
            right.Value = res;
            return right;
        }

        public Residue<T> Power(int power)
        {
            var res = Modulo.Power(Value, power);
            if (IsImmutable)
            {
                return new Residue<T>(res, Modulo);
            }
            Value = res;
            return this;
        }
        public Residue<T> Power(T power)
        {
            var res = Modulo.Power(Value, power);
            if (IsImmutable)
            {
                return new Residue<T>(res, Modulo);
            }
            Value = res;
            return this;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Modulo, Value);
        }
        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not Residue<T>)
                return false;
            return this == (Residue<T>)obj;
        }
        public bool Equals(Residue<T>? other)
        {
            if (other is null) return false;
            return this == other;
        }
    }
}
