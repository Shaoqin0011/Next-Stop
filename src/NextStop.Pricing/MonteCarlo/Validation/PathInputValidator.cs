using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Setting;

namespace NextStop.Pricing.MonteCarlo.Validation;

public static class PathInputValidator
{
    public static void Validate(
        Option option,
        MarketData market,
        PathMonteCarloSettings settings)
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(market);
        ArgumentNullException.ThrowIfNull(settings);

        if (option.Strike <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(option.Strike),
                "Strike must be positive.");
        }

        if (option.TimeToMaturity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(option.TimeToMaturity),
                "Time to maturity must be positive.");
        }

        if (market.Spot <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(market.Spot),
                "Spot must be positive.");
        }

        if (market.Volatility <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(market.Volatility),
                "Volatility must be positive.");
        }
    }
}
