namespace NextStop.Pricing.MonteCarlo.Setting;

public class MonteCarloSettings
{
    public int NumberOfPaths { get; }
    public int? RandomSeed { get; }

    public MonteCarloSettings(
        int numberOfPaths,
        int? randomSeed = null)
    {
        if (numberOfPaths <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfPaths),
                "Number of paths must be positive.");
        }

        NumberOfPaths = numberOfPaths;
        RandomSeed = randomSeed;
    }
}