using NextStop.Pricing.MonteCarlo.Random;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class GaussianRandomGeneratorTests
{
    [Fact]
    public void SameSeed_ShouldGenerateSameSequence()
    {
        var firstGenerator = new GaussianRandomGenerator(seed: 42);
        var secondGenerator = new GaussianRandomGenerator(seed: 42);

        double[] firstSequence = Enumerable.Range(0, 10)
            .Select(_ => firstGenerator.Next())
            .ToArray();
        double[] secondSequence = Enumerable.Range(0, 10)
            .Select(_ => secondGenerator.Next())
            .ToArray();

        Assert.Equal(firstSequence, secondSequence);
    }

    [Fact]
    public void Fill_ShouldPopulateTheProvidedArray()
    {
        var generator = new GaussianRandomGenerator(seed: 42);
        double[] values = Enumerable.Repeat(double.NaN, 16).ToArray();

        generator.Fill(values);

        Assert.Equal(16, values.Length);
        Assert.All(values, value => Assert.True(double.IsFinite(value)));
    }
}
