using System.Numerics;

namespace EllipticCurvesMath
{
    internal class PointIsNotAffineException<T> : Exception
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public PointCoordinates<T> point {  get; private set; }
        public EllipticCurve<T> curve { get; private set; }

        public PointIsNotAffineException(PointCoordinates<T> point, EllipticCurve<T> curve) : base($"{point} is not affine to {curve}")
        {
            this.point = point;
            this.curve = curve;
        }
    }
}
