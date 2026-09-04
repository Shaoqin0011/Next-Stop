using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using MathNet.Numerics.Distributions;
using NextStop.Pricing.BlackScholes.Validation;

namespace NextStop.Pricing.BlackScholes;

public sealed class BlackScholesPricingEngine
{
    public double Price(
        Option option,
        MarketData market)
    {
        BlackScholesInputValidator.Validate(option, market);

        double s = market.Spot;
        double k = option.Strike;
        double r = market.RiskFreeRate;
        double vol = market.Volatility;
        double tau = option.TimeToMaturity;
        double y = market.DividendYield;

        double d1 = BlackScholesMath.GetD1(s, k, r, vol, tau, y);
        double d2 = BlackScholesMath.GetD2(d1, vol, tau);

        if (option.Type == OptionType.Call)
        {
            return s * Math.Exp(-y * tau) * Normal.CDF(0.0, 1.0, d1) - k * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, d2);
        } else if (option.Type == OptionType.Put)
        {
            return k * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, -d2) - s * Math.Exp(-y * tau) * Normal.CDF(0.0, 1.0, -d1);
        }else
        {
            throw new ArgumentException("Invalid option type");
        }
    }
}
