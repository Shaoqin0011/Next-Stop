using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Setting;

namespace NextStop.Pricing.MonteCarlo.Validation;

public static class TerminalPriceInputValidator
{
    public static void Validate(
        Option option,
        MarketData market,
        MonteCarloSettings settings)
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(market);
        ArgumentNullException.ThrowIfNull(settings);

        if (option is AmericanOption && market.DividendYield != 0.0)
        {
            throw new NotSupportedException(
                "PriceWithTerminalPrice does not support dividend-paying American options.");
        }

        ValidateOptionAndMarket(option, market);
    }

    private static void ValidateOptionAndMarket(Option option, MarketData market)
    {
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
