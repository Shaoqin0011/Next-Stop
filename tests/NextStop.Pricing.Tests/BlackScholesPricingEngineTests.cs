using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;

namespace NextStop.Pricing.Tests;

public class BlackScholesPricingEngineTests
{
    [Fact]
    public void CallPrice_ShouldMatchKnownBlackScholesValue()
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

        var engine = new BlackScholesPricingEngine();

        double price = engine.Price(option, market);

        Assert.Equal(10.4506, price, precision: 4);
    }

    [Fact]
    public void PutPrice_ShouldMatchKnownBlackScholesValue()
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

        var engine = new BlackScholesPricingEngine();

        double price = engine.Price(option, market);

        Assert.Equal(5.5735, price, precision: 4);
    }

    [Fact]
    public void CallAndPut_ShouldSatisfyPutCallParity()
    {
        double strike = 100.0;
        double timeToMaturity = 1.0;

        var call = new EuropeanOption(
            strike: strike,
            timeToMaturity: timeToMaturity,
            type: OptionType.Call
        );

        var put = new EuropeanOption(
            strike: strike,
            timeToMaturity: timeToMaturity,
            type: OptionType.Put
        );

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0
        );

        var engine = new BlackScholesPricingEngine();

        double callPrice = engine.Price(call, market);
        double putPrice = engine.Price(put, market);

        double leftSide =
            callPrice - putPrice;

        double rightSide =
            market.Spot
            - strike * Math.Exp(
                -market.RiskFreeRate * timeToMaturity
            );

        Assert.Equal(
            rightSide,
            leftSide,
            precision: 6
        );
    }
}