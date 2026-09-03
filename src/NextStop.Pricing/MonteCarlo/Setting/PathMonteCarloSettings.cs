namespace NextStop.Pricing.MonteCarlo.Setting;

public sealed class PathMonteCarloSettings : MonteCarloSettings
{
    public int NumberOfTimeSteps { get; }

    public PathMonteCarloSettings(
        int numberOfPaths,
        int numberOfTimeSteps,
        int? randomSeed = null)
        : base(numberOfPaths, randomSeed)
    {
        if (numberOfTimeSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfTimeSteps),
                "Number of time steps must be positive.");
        }

        NumberOfTimeSteps = numberOfTimeSteps;
    }
}