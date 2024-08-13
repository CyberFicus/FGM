using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EllipticCurvesMath
{
    public struct PointCoordinates<T> : IEquatable<PointCoordinates<T>>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public T X { readonly get; private set; }
        public T Y { readonly get; private set; }
        
        public PointCoordinates(T x, T y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is not PointCoordinates<T>) return false;
            return Equals((PointCoordinates<T>)obj);
        }
        public bool Equals(PointCoordinates<T>? other)
        {
            if (other is null) return false;
            if (X != other.Value.X) return false;
            if (Y != other.Value.Y) return false;
            return true;
        }
        bool IEquatable<PointCoordinates<T>>.Equals(PointCoordinates<T> other)
        {
            if (X != other.X) return false;
            if (Y != other.Y) return false;
            return true;
        }
        public static bool operator ==(PointCoordinates<T>? left, PointCoordinates<T>? right)
        {
            if (left is null)
            {
                if (right is null) return true;
                return false;
            }
            return left.Equals(right);
        }
        public static bool operator !=(PointCoordinates<T>? left, PointCoordinates<T>? right)
        {
            return !(left == right);
        }
    }
}
