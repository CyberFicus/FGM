using System.Numerics;

namespace ModularArithmetic
{
    /// <summary>
    /// To be thrown when fraction's denominator and modulus are not coprime
    /// <para>Carries common divisor (preferably GCD) of denominator and modulus in CommonDivisor property</para>
    /// </summary>
    public class ModularDivisionException<T> : Exception
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public T CommonDivisor { get; private set; }

        public ModularDivisionException(T div, string str = "") : base(str)
        {
            CommonDivisor = div;
        }
    }
}
