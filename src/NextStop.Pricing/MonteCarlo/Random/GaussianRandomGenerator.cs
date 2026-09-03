using MathNet.Numerics.Distributions;

namespace NextStop.Pricing.MonteCarlo.Random;

public sealed class GaussianRandomGenerator
{
    private readonly System.Random _random;

    public GaussianRandomGenerator(int? seed = null)
    {
        _random = seed.HasValue
            ? new System.Random(seed.Value)
            : new System.Random();
    }

    public double Next()
    {
        return Normal.Sample(_random, 0.0, 1.0);
    }

    public void Fill(double[] values)
    {
        Normal.Samples(_random, values, 0.0, 1.0);
    }
}