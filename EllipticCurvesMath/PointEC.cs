using System.Numerics;

namespace EllipticCurvesMath
{
    public class PointEC<T, TOrder> : IEquatable<PointEC<T, TOrder>>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TOrder : IBinaryInteger<TOrder>
    {
        public PointCoordinates<T>? Coordinates { get; private set; }
        public EllipticCurve<T> Curve { get; private set; }
        public bool IsAffine { get; private set; }
        
        public PointEC(EllipticCurve<T> curve, PointCoordinates<T>? coordinates = null)
        {
            Curve = curve;
            Coordinates = coordinates;
            IsAffine = curve.IsAffine(Coordinates);
            _ThrowExcIfNotAffine(this);
        }
        public PointEC(EllipticCurve<T> curve, T x, T y)
        {
            Curve = curve;
            Coordinates = new PointCoordinates<T>(x, y);
            IsAffine = curve.IsAffine(Coordinates);
            _ThrowExcIfNotAffine(this);
        }

        public override string ToString()
        {
            var pnt = Coordinates is null ? "Inf" : Coordinates.ToString();
            return $"{pnt} over {Curve}";
        }

        private static void _ThrowExcIfNotAffine(PointEC<T, TOrder> point)
        {
            if (point.IsAffine) return;
            throw new PointIsNotAffineException<T>(point.Coordinates.Value, point.Curve);
        }

        public PointEC<T, TOrder> Inverse()
        {
            return new PointEC<T, TOrder>(Curve, Curve.Inverse(Coordinates));
        }
        
        public static PointEC<T, TOrder> operator+(PointEC<T, TOrder> left, PointCoordinates<T>? right)
        {
            return new PointEC<T, TOrder>(left.Curve, left.Curve.Add(left.Coordinates, right));
        }
        public static PointEC<T, TOrder> operator +(PointCoordinates<T>? left, PointEC<T, TOrder> right)
        {
            return right + left;
        }
        public static PointEC<T, TOrder> operator+(PointEC<T, TOrder> left, PointEC<T, TOrder> right)
        {
            if (left.Curve != right.Curve) throw new InvalidOperationException($"Passed points belong to different elliptic curves");

            return left + right.Coordinates;
        }

        public static PointEC<T, TOrder> operator*(PointEC<T, TOrder> point, T multiplier)
        {
            return new PointEC<T, TOrder>(point.Curve, point.Curve.Multiply(multiplier, point.Coordinates));
        }
        public static PointEC<T, TOrder> operator *(T multiplier, PointEC<T, TOrder> point)
        {
            return new PointEC<T, TOrder>(point.Curve, point.Curve.Multiply(multiplier, point.Coordinates));
        }
        public static PointEC<T, TOrder> operator *(PointEC<T, TOrder> point, TOrder multiplier)
        {
            return new PointEC<T, TOrder>(point.Curve, point.Curve.Multiply(multiplier, point.Coordinates));
        }
        public static PointEC<T, TOrder> operator *(TOrder multiplier, PointEC<T, TOrder> point)
        {
            return new PointEC<T, TOrder>(point.Curve, point.Curve.Multiply(multiplier, point.Coordinates));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Curve, Coordinates);
        }
        public override bool Equals(object? other)
        {
            if (other is null) return false;
            if (other is not PointEC<T, TOrder>) return false;
            return Equals((PointEC<T, TOrder>)other);
        }
        public bool Equals(PointEC<T, TOrder>? other)
        {
            if (other is null) return false;
            if (Curve != other.Curve) return false;
            if (Coordinates != other.Coordinates) return false;
            return true;
        }
        public int EqualOrInverse(PointEC<T, TOrder> otherPoint)
        {
            return Curve.AreEqualOrInverse(this.Coordinates, otherPoint.Coordinates);
        }
        public int EqualOrInverse(PointCoordinates<T> otherPoint)
        {
            return Curve.AreEqualOrInverse(this.Coordinates, otherPoint);
        }
    }
}
