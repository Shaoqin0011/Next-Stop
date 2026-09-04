using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;

namespace NextStop.Pricing.Tests.BlackScholes;

public class BlackScholesGreeksCalculatorTests
{
    private readonly BlackScholesGreeksCalculator _calculator = new();

    [Fact]
    public void CallDelta_ShouldMatchKnownValue()
    {
        double delta = _calculator.Delta(CreateOption(OptionType.Call), CreateMarket());

        Assert.Equal(0.6368, delta, precision: 4);
    }

    [Fact]
    public void PutDelta_ShouldMatchKnownValue()
    {
        double delta = _calculator.Delta(CreateOption(OptionType.Put), CreateMarket());

        Assert.Equal(-0.3632, delta, precision: 4);
    }

    [Fact]
    public void Gamma_ShouldBePositive()
    {
        double gamma = _calculator.Gamma(CreateOption(OptionType.Call), CreateMarket());

        Assert.True(gamma > 0.0);
    }

    [Fact]
    public void Vega_ShouldBePositive()
    {
        double vega = _calculator.Vega(CreateOption(OptionType.Call), CreateMarket());

        Assert.True(vega > 0.0);
    }

    [Fact]
    public void CallAndPut_ShouldHaveTheSameGamma()
    {
        MarketData market = CreateMarket();

        double callGamma = _calculator.Gamma(CreateOption(OptionType.Call), market);
        double putGamma = _calculator.Gamma(CreateOption(OptionType.Put), market);

        Assert.Equal(callGamma, putGamma, precision: 10);
    }

    private static EuropeanOption CreateOption(OptionType type) => new(100.0, 1.0, type);

    private static MarketData CreateMarket() =>
        new(spot: 100.0, riskFreeRate: 0.05, volatility: 0.20, dividendYield: 0.0);
}
