using NextStop.Domain.Instruments;
using NextStop.Domain.Market;

namespace NextStop.Pricing.BlackScholes.Validation;

public static class BlackScholesInputValidator
{
    public static void Validate(
        Option option,
        MarketData market)
    {
        bool isSupported =
            (option is EuropeanOption)
            ||
            (option is AmericanOption && option.Type == OptionType.Call && market.DividendYield == 0.0);

        if (!isSupported)
        {
            throw new NotSupportedException(
                "Black-Scholes engine supports European options and non-dividend-paying American calls only.");
        }

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