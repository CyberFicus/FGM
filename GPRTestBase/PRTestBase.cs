using System.Numerics;

namespace PRTestBase
{
    /// <summary>
    /// Provides inheritable base functionality for pseudo random tests
    /// <para>ApplySeed is to be used before pseudo random test every test for their results to be repeatable</para>
    /// <para>PRTestIterations is to define how many iterations of testing should be in RunIterations</para>
    /// </summary>
    public class PRTestBase
    {
        /// <summary>
        /// To be seeded with a constant and used in all tests with pseudo random input
        /// </summary>
        public static Random SharedRandom { get; protected set; }
        /// <summary>
        /// Defines how many iterations will be done inside RunIterations
        /// </summary>
        public static uint PRTestIterations { get; protected set; }
        /// <summary>
        /// A constant value to seed Shared Random
        /// </summary>
        public static int SeedValue { get; protected set; }
        /// <summary>
        /// Must be invoked before the beginning of everey test that uses SharedRandom
        /// </summary>
        public void ApplySeed()
        {
            SharedRandom = new Random(SeedValue);
        }
        
        public delegate void Iteration(object?[]? args);
        /// <summary>
        /// Runs iteration of pseudo random test defined number of times. Allows to pass arguments
        /// </summary>
        /// <param name="arguments">Set of arguments passed to iteration</param>
        public virtual void RunIterations(Iteration iteration, object?[]? arguments = null)
        {
            for (int i = 0; i < PRTestIterations; i++)
            {
                iteration(arguments);
            }
        }
        public delegate void IterationNoArgs();
        /// <summary>
        /// Runs iteration of pseudo random test defined number of times with no arguments.
        /// </summary>
        public virtual void RunIterations(IterationNoArgs iteration)
        {
            for (int i = 0; i < PRTestIterations; i++)
            {
                iteration();
            }
        }
    }
}
