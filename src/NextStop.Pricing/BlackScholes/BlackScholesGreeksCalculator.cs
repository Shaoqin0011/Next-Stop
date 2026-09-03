using MathNet.Numerics.Distributions;
using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes.Validation;
using NextStop.Pricing.Results;
namespace NextStop.Pricing.BlackScholes;

public sealed class BlackScholesGreeksCalculator
{

    public Greeks CalculateAll(
        Option option,
        MarketData market)
    {
        BlackScholesInputValidator.Validate(option, market);

        return new Greeks(
            Delta(option, market, true),
            Gamma(option, market, true),
            Theta(option, market, true),
            Vega(option, market, true),
            Rho(option, market, true)
        );
    }
    
    public double Delta(
        Option option,
        MarketData market,
        bool validated = false)
    {
        if (!validated)
        {
            BlackScholesInputValidator.Validate(option, market);
        }

        double s = market.Spot;
        double k = option.Strike;
        double r = market.RiskFreeRate;
        double vol = market.Volatility;
        double tau = option.TimeToMaturity;
        double y = market.DividendYield;

        double d1 = BlackScholesMath.GetD1(s, k, r, vol, tau, y);

        if (option.Type == OptionType.Call)
        {
            return Math.Exp(-y * tau) * Normal.CDF(0.0, 1.0, d1);
        }
        else if (option.Type == OptionType.Put)
        {
            return Math.Exp(-y * tau) * (Normal.CDF(0.0, 1.0, d1) - 1);
        }
        else
        {
            throw new ArgumentException("Invalid option type");
        }
    }

    public double Gamma(
        Option option,
        MarketData market,
        bool validated = false)
    {
        if (!validated)
        {
            BlackScholesInputValidator.Validate(option, market);
        }

        double s = market.Spot;
        double k = option.Strike;
        double r = market.RiskFreeRate;
        double vol = market.Volatility;
        double tau = option.TimeToMaturity;
        double y = market.DividendYield;

        double d1 = BlackScholesMath.GetD1(s, k, r, vol, tau, y);

        return Math.Exp(-y * tau) * Normal.PDF(0.0, 1.0, d1) / (s * vol * Math.Sqrt(tau));
    }

    public double Theta(
        Option option,
        MarketData market,
        bool validated = false)
    {
        if (!validated)
        {
            BlackScholesInputValidator.Validate(option, market);
        }

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
            return -s * Math.Exp(-y * tau) * Normal.PDF(0.0, 1.0, d1) * vol / (2 * Math.Sqrt(tau))
                   - r * k * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, d2)
                   + y * s * Math.Exp(-y * tau) * Normal.CDF(0.0, 1.0, d1);
        }
        else if (option.Type == OptionType.Put)
        {
            return -s * Math.Exp(-y * tau) * Normal.PDF(0.0, 1.0, d1) * vol / (2 * Math.Sqrt(tau))
                   + r * k * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, -d2)
                   - y * s * Math.Exp(-y * tau) * Normal.CDF(0.0, 1.0, -d1);
        }
        else
        {
            throw new ArgumentException("Invalid option type");
        }
    }

    public double Vega(
        Option option,
        MarketData market,
        bool validated = false)
    {
        if (!validated)
        {
            BlackScholesInputValidator.Validate(option, market);
        }

        double s = market.Spot;
        double k = option.Strike;
        double r = market.RiskFreeRate;
        double vol = market.Volatility;
        double tau = option.TimeToMaturity;
        double y = market.DividendYield;

        double d1 = BlackScholesMath.GetD1(s, k, r, vol, tau, y);

        return s * Math.Exp(-y * tau) * Normal.PDF(0.0, 1.0, d1) * Math.Sqrt(tau);
    }

    public double Rho(
        Option option,
        MarketData market,
        bool validated = false)
    {
        if (!validated)
        {
            BlackScholesInputValidator.Validate(option, market);
        }

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
            return k * tau * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, d2);
        }
        else if (option.Type == OptionType.Put)
        {
            return -k * tau * Math.Exp(-r * tau) * Normal.CDF(0.0, 1.0, -d2);
        }
        else
        {
            throw new ArgumentException("Invalid option type");
        }
    }



}