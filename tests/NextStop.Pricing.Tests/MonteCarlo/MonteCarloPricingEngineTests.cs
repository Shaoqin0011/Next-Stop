using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;
using NextStop.Pricing.MonteCarlo.Setting;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class MonteCarloPricingEngineTests
{
    private const int NumberOfPaths = 100_000;
    private const int Seed = 42;
    private const double Tolerance = 0.15;

    private readonly global::MonteCarloPricingEngine _monteCarloEngine = new();
    private readonly BlackScholesPricingEngine _blackScholesEngine = new();

    [Fact]
    public void EuropeanCallPrice_ShouldBeCloseToBlackScholesPrice()
    {
        EuropeanOption option = CreateOption(OptionType.Call);
        MarketData market = CreateMarket();

        double expected = _blackScholesEngine.Price(option, market);
        double actual = _monteCarloEngine.PriceWithTerminalPrice(option, market, CreateSettings());

        Assert.InRange(actual, expected - Tolerance, expected + Tolerance);
    }

    [Fact]
    public void EuropeanPutPrice_ShouldBeCloseToBlackScholesPrice()
    {
        EuropeanOption option = CreateOption(OptionType.Put);
        MarketData market = CreateMarket();

        double expected = _blackScholesEngine.Price(option, market);
        double actual = _monteCarloEngine.PriceWithTerminalPrice(option, market, CreateSettings());

        Assert.InRange(actual, expected - Tolerance, expected + Tolerance);
    }

    [Fact]
    public void SameSeedAndInputs_ShouldProduceSamePrice()
    {
        EuropeanOption option = CreateOption(OptionType.Call);
        MarketData market = CreateMarket();

        double firstPrice = _monteCarloEngine.PriceWithTerminalPrice(option, market, CreateSettings());
        double secondPrice = _monteCarloEngine.PriceWithTerminalPrice(option, market, CreateSettings());

        Assert.Equal(firstPrice, secondPrice);
    }

    private static EuropeanOption CreateOption(OptionType type) =>
        new(strike: 100.0, timeToMaturity: 1.0, type);

    private static MarketData CreateMarket() =>
        new(spot: 100.0, riskFreeRate: 0.05, volatility: 0.20, dividendYield: 0.0);

    private static MonteCarloSettings CreateSettings() => new(NumberOfPaths, Seed);
}
