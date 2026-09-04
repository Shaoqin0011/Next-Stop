using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;
using NextStop.Pricing.MonteCarlo.Setting;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class LongstaffSchwartzPricingEngineTests
{
    private const int Seed = 42;

    private readonly global::MonteCarloPricingEngine _longstaffSchwartzEngine = new();
    private readonly BlackScholesPricingEngine _blackScholesEngine = new();

    [Fact]
    public void AmericanPutPrice_ShouldBeAtLeastEuropeanPutPrice()
    {
        MarketData market = CreateMarket();
        var americanPut = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Put);
        var europeanPut = new EuropeanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Put);

        double americanPrice = _longstaffSchwartzEngine.PriceWithPath(americanPut, market, CreateBenchmarkSettings());
        double europeanPrice = _blackScholesEngine.Price(europeanPut, market);

        Assert.True(americanPrice >= europeanPrice);
    }

    [Fact]
    public void NonDividendAmericanCallPrice_ShouldBeCloseToEuropeanCallPrice()
    {
        MarketData market = CreateMarket();
        var americanCall = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);
        var europeanCall = new EuropeanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);

        double americanPrice = _longstaffSchwartzEngine.PriceWithPath(americanCall, market, CreateBenchmarkSettings());
        double europeanPrice = _blackScholesEngine.Price(europeanCall, market);

        Assert.InRange(americanPrice, europeanPrice - 0.25, europeanPrice + 0.25);
    }

    [Fact]
    public void AmericanOptionPrice_ShouldBeAtLeastIntrinsicValue()
    {
        var option = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Put);
        MarketData market = CreateMarket(spot: 80.0);

        double price = _longstaffSchwartzEngine.PriceWithPath(option, market, CreateBenchmarkSettings());

        Assert.True(price >= option.Payoff(market.Spot));
    }

    [Fact]
    public void SameSeedAndSettings_ShouldProduceSamePrice()
    {
        var option = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Put);
        MarketData market = CreateMarket();
        var settings = new PathMonteCarloSettings(numberOfPaths: 10_000, numberOfTimeSteps: 25, randomSeed: Seed);

        double firstPrice = _longstaffSchwartzEngine.PriceWithPath(option, market, settings);
        double secondPrice = _longstaffSchwartzEngine.PriceWithPath(option, market, settings);

        Assert.Equal(firstPrice, secondPrice);
    }

    [Fact]
    public void PriceWithPath_WhenThereAreNoInTheMoneyPaths_ShouldNotThrow()
    {
        var option = new AmericanOption(strike: 0.01, timeToMaturity: 1.0, OptionType.Put);
        MarketData market = CreateMarket(volatility: 0.01);
        var settings = new PathMonteCarloSettings(numberOfPaths: 100, numberOfTimeSteps: 10, randomSeed: Seed);

        Exception? exception = Record.Exception(() =>
            _longstaffSchwartzEngine.PriceWithPath(option, market, settings));

        Assert.Null(exception);
    }

    private static PathMonteCarloSettings CreateBenchmarkSettings() =>
        new(numberOfPaths: 50_000, numberOfTimeSteps: 50, randomSeed: Seed);

    private static MarketData CreateMarket(double spot = 100.0, double volatility = 0.20) =>
        new(spot, riskFreeRate: 0.05, volatility, dividendYield: 0.0);
}
