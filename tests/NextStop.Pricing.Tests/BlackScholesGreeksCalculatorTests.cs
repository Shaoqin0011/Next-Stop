using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;

namespace NextStop.Pricing.Tests;

public class BlackScholesGreeksCalculatorTests
{
    [Fact]
    public void CallDelta_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Call
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double delta = calculator.Delta(option, market);

        Assert.Equal(0.6368, delta, precision: 4);
    }

    [Fact]
    public void PutDelta_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Put
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double delta = calculator.Delta(option, market);

        Assert.Equal(-0.3632, delta, precision: 4);
    }

    [Fact]
    public void Gamma_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Call
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double gamma = calculator.Gamma(option, market);

        Assert.Equal(0.0188, gamma, precision: 4);
    }

    [Fact]
    public void Vega_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Call
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double vega = calculator.Vega(option, market);

        Assert.Equal(37.5240, vega, precision: 4);
    }

    [Fact]
    public void CallRho_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Call
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double rho = calculator.Rho(option, market);

        Assert.Equal(53.2325, rho, precision: 4);
    }

    [Fact]
    public void CallTheta_ShouldMatchKnownValue()
    {
        var option = new EuropeanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Call
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var calculator = new BlackScholesGreeksCalculator();

        double theta = calculator.Theta(option, market);

        Assert.Equal(-6.4140, theta, precision: 4);
    }
}