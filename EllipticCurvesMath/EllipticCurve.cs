using ModularArithmetic;
using System.Numerics;
using GenericMathUtilities;
using System.Security.Cryptography;

namespace EllipticCurvesMath
{
    public class EllipticCurve<T> : IEquatable<EllipticCurve<T>>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public T A { get; private set; }
        public T B { get; private set; }
        public T P { get { return ModP.Modulus; } }
        public Modulo<T> ModP { get; private set; }

        public EllipticCurve(T a, T b, Modulo<T> modP)
        {
            ModP = modP;
            this.A = a % P; 
            this.B = b % P;
        }
        public EllipticCurve(T a, T b, T p)
        {
            this.A = a % p; 
            this.B = b % p;
            ModP = new Modulo<T>(p);
        }

        public override string ToString()
        {
            return $"y^2 = x^3 + {A}x + {B} {ModP}";
        }

        public PointEC<T, TOrder> Point<TOrder>(PointCoordinates<T>? coordinates = null)
            where TOrder : IBinaryInteger<TOrder>
        {
            return new PointEC<T, TOrder>(this, coordinates);
        }
        public PointEC<T, TOrder> Point<TOrder>(T x, T y)
            where TOrder : IBinaryInteger<TOrder>
        {
            return new PointEC<T, TOrder>(this, x, y);
        }

        public PointCoordinates<T>? Inverse(PointCoordinates<T>? point)
        {
            if (point == null || point.Value.Y == T.Zero) return point;
            return new PointCoordinates<T>(point.Value.X, ModP.Minus(point.Value.Y));
        }
        public PointCoordinates<T>? Add(PointCoordinates<T>? point1, PointCoordinates<T>? point2)
        {
            if (point1 is null) return point2;
            if (point2 is null) return point1;

            T d, newX, newY;

            if (point1.Value.X == point2.Value.X)
            {
                if (point1.Value.Y != point2.Value.Y) 
                {
                    if (point1.Value.Y == ModP.Minus(point2.Value.Y))
                        return null;
                    throw new InvalidOperationException($"{point1} and {point2} have same X and different Y, but are not inverse for curve {this}");
                }
                if (point1.Value.Y == T.Zero) return null;

                // d = (3* X1^2 + A) / 2 * Y1
                d = ModP.Divide(
                    ModP.Add(
                        ModP.Multiply(
                           3,
                           ModP.Power(point1.Value.X, 2)
                        ),
                        A
                    ),
                    ModP.Multiply(2, point1.Value.Y)
                );
                // X3 = d^2 - 2*X1
                newX = ModP.Subtract(
                    ModP.Power(d, 2),
                    ModP.Multiply(2, point1.Value.X)
                );
                // Y3 =  d(X1 - X3) - Y1
                newY = ModP.Subtract(
                    ModP.Multiply(
                        d,
                        ModP.Subtract(point1.Value.X, newX)
                    ),
                    point1.Value.Y
                );
            }
            else
            {
                // d = ( Y1 - Y1 ) / ( X2 - X2 ) 
                d = ModP.Divide(
                    ModP.Subtract(point1.Value.Y, point2.Value.Y), 
                    ModP.Subtract(point1.Value.X, point2.Value.X)
                );
                // X3 = d^2 - X1 - X2
                newX = ModP.Subtract(
                    ModP.Power(d, 2), 
                    ModP.Add(point1.Value.X, point2.Value.X)
                );
                // Y3 =  d(X1 - X3) - Y1
                newY = ModP.Subtract(
                    ModP.Multiply(
                        d, 
                        ModP.Subtract(
                            point1.Value.X, 
                            newX)
                        ),
                    point1.Value.Y
                );
            }

            return new PointCoordinates<T>(newX, newY);
        }
        public PointCoordinates<T>? Multiply<TInteger>(TInteger multiplier, PointCoordinates<T>? point)
            where TInteger : IBinaryInteger<TInteger>
        {
            if (point is null || multiplier == TInteger.Zero) { return null; }
            if (multiplier < TInteger.Zero)
                return Multiply(-multiplier, Inverse(point));

            /*
            if (CurveOrder is not null)
                multiplier %= (D)orderAsD;
            //*/

            PointCoordinates<T>? res = null, buf = point;

            if (TInteger.IsOddInteger(multiplier)) res = point;
            multiplier >>= 1;
            
            while (multiplier > TInteger.Zero)
            {
                buf = Add(buf, buf);
                
                if (TInteger.IsOddInteger(multiplier)) 
                    res = Add(res, buf);
                multiplier >>= 1;
            }

            return res;
        }
        public PointCoordinates<T>? Multiply<TInteger>(TInteger multiplier, PointCoordinates<T>? point, TInteger order)
            where TInteger: IBinaryInteger<TInteger>
        {
            return Multiply(multiplier % order, point);
        }

        /// <summary>
        /// Calculates y^2
        /// </summary>
        public T GetLeftPart(T y) { return ModP.Power(y, 2); }
        /// <summary>
        /// Calculates x^3 + ax + b
        /// </summary>
        public T GetRightPart(T x)
        {
            T res = ModP.Multiply(x, x); // x^2
            res = ModP.Add(res, A); // x^2+a
            res = ModP.Multiply(res, x); // x^3 + ax
            res = ModP.Add(res, B); // x^3 + ax + b
            return res;
        }
        /// <summary>
        /// Returns true if point with passed coordinates belongs to the elliptic curves. Otherwise returns false
        /// </summary>
        public bool IsAffine(PointCoordinates<T>? point)
        {
            if (point is null) return true;
            return (GetLeftPart(point.Value.Y) == GetRightPart(point.Value.X));
        }
        public PointCoordinates<T> GetCoordinatesFromX(T x)
        {
            var y = ModP.GetSqareRoot(GetRightPart(x));
            var buf = ModP.Minus(y);
            if (buf < y)
                y = buf;
            return new PointCoordinates<T>(x, y);
        }
        /// <summary>
        /// Returns 1 if points are equal, returns -1 if they are inverse, returns 0 if they are neither equal nor inverseS
        /// </summary>
        public int AreEqualOrInverse(PointCoordinates<T>? point1, PointCoordinates<T>? point2)
        {
            if (point1 is null)
            {
                if (point2 is null) return 1;
                return 0;
            }
            if (point2 is null) return 0;
            if (point1.Value.X != point2.Value.X) return 0;
            if (point1.Value.Y == point2.Value.Y) return 1;
            if (point1.Value.Y == ModP.Minus(point2.Value.Y)) return -1;
            return 0;
        }

        public PointCoordinates<T> GetRandomPoint(Random? random = null)
        {
            var rnd = (random is null) ? new Random() : random;
            var x = GUMU<T>.Random(P, rnd);
            while (ModP.IsQuadraticResidue(GetRightPart(x)) is false)
            {
                x = GUMU<T>.Random(P, rnd);
            }

            return GetCoordinatesFromX(x);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, ModP);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not EllipticCurve<T>) return false;
            return Equals((EllipticCurve<T>)obj);
        }
        public bool Equals(EllipticCurve<T>? other)
        {
            if (other is null) return false;
            if (ModP != other.ModP) return false;
            if (A != other.A) return false;
            if (B != other.B) return false;
            return true;
        }
        public static bool operator ==(EllipticCurve<T> lhs, EllipticCurve<T> rhs)
        {
            if (lhs.A != rhs.A) return false;
            if (lhs.B != rhs.B) return false;
            if (lhs.P != rhs.P) return false;
            return true;
        }
        public static bool operator !=(EllipticCurve<T> lhs, EllipticCurve<T> rhs)
        {
            return !(lhs == rhs);
        }
    }
}