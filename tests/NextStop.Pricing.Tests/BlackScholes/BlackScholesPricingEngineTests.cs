using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;

namespace NextStop.Pricing.Tests.BlackScholes;

public class BlackScholesPricingEngineTests
{
    private readonly BlackScholesPricingEngine _engine = new();

    [Fact]
    public void NonDividendPayingAmericanCall_ShouldMatchEuropeanCall()
    {
        MarketData market = CreateMarket();
        var americanCall = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);
        var europeanCall = new EuropeanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);

        double americanPrice = _engine.Price(americanCall, market);
        double europeanPrice = _engine.Price(europeanCall, market);

        Assert.Equal(europeanPrice, americanPrice);
    }

    [Fact]
    public void CallPrice_ShouldMatchKnownBlackScholesValue()
    {
        double price = _engine.Price(CreateOption(OptionType.Call), CreateMarket());

        Assert.Equal(10.4506, price, precision: 4);
    }

    [Fact]
    public void PutPrice_ShouldMatchKnownBlackScholesValue()
    {
        double price = _engine.Price(CreateOption(OptionType.Put), CreateMarket());

        Assert.Equal(5.5735, price, precision: 4);
    }

    [Fact]
    public void CallAndPut_ShouldSatisfyPutCallParity()
    {
        const double strike = 100.0;
        const double timeToMaturity = 1.0;
        MarketData market = CreateMarket();

        double callPrice = _engine.Price(CreateOption(OptionType.Call, strike, timeToMaturity), market);
        double putPrice = _engine.Price(CreateOption(OptionType.Put, strike, timeToMaturity), market);
        double expectedDifference = market.Spot - strike * Math.Exp(-market.RiskFreeRate * timeToMaturity);

        Assert.Equal(expectedDifference, callPrice - putPrice, precision: 6);
    }

    [Fact]
    public void CallPrice_ShouldIncreaseWhenSpotIncreases()
    {
        EuropeanOption option = CreateOption(OptionType.Call);

        double lowerSpotPrice = _engine.Price(option, CreateMarket(spot: 90.0));
        double higherSpotPrice = _engine.Price(option, CreateMarket(spot: 110.0));

        Assert.True(higherSpotPrice > lowerSpotPrice);
    }

    [Fact]
    public void PutPrice_ShouldDecreaseWhenSpotIncreases()
    {
        EuropeanOption option = CreateOption(OptionType.Put);

        double lowerSpotPrice = _engine.Price(option, CreateMarket(spot: 90.0));
        double higherSpotPrice = _engine.Price(option, CreateMarket(spot: 110.0));

        Assert.True(higherSpotPrice < lowerSpotPrice);
    }

    [Fact]
    public void CallPrice_ShouldSatisfyNoArbitrageBounds()
    {
        EuropeanOption option = CreateOption(OptionType.Call);
        MarketData market = CreateMarket();
        double discountedSpot = market.Spot * Math.Exp(-market.DividendYield * option.TimeToMaturity);
        double discountedStrike = option.Strike * Math.Exp(-market.RiskFreeRate * option.TimeToMaturity);
        double lowerBound = Math.Max(discountedSpot - discountedStrike, 0.0);

        double price = _engine.Price(option, market);

        Assert.InRange(price, lowerBound, discountedSpot);
    }

    [Fact]
    public void PutPrice_ShouldSatisfyNoArbitrageBounds()
    {
        EuropeanOption option = CreateOption(OptionType.Put);
        MarketData market = CreateMarket();
        double discountedSpot = market.Spot * Math.Exp(-market.DividendYield * option.TimeToMaturity);
        double discountedStrike = option.Strike * Math.Exp(-market.RiskFreeRate * option.TimeToMaturity);
        double lowerBound = Math.Max(discountedStrike - discountedSpot, 0.0);

        double price = _engine.Price(option, market);

        Assert.InRange(price, lowerBound, discountedStrike);
    }

    private static EuropeanOption CreateOption(OptionType type, double strike = 100.0, double timeToMaturity = 1.0) =>
        new(strike, timeToMaturity, type);

    private static MarketData CreateMarket(double spot = 100.0) =>
        new(spot, riskFreeRate: 0.05, volatility: 0.20, dividendYield: 0.0);
}
