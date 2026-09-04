using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes.Validation;

namespace NextStop.Pricing.Tests.Validation;

public class PricingInputValidatorTests
{
    [Fact]
    public void Validate_ShouldThrowWhenSpotIsNotPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(), CreateMarket(spot: 0.0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(), CreateMarket(spot: -1.0)));
    }

    [Fact]
    public void Validate_ShouldThrowWhenStrikeIsNotPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(strike: 0.0), CreateMarket()));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(strike: -1.0), CreateMarket()));
    }

    [Fact]
    public void Validate_ShouldThrowWhenVolatilityIsNotPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(), CreateMarket(volatility: 0.0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(), CreateMarket(volatility: -0.01)));
    }

    [Fact]
    public void Validate_ShouldThrowWhenTimeToMaturityIsNotPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(timeToMaturity: 0.0), CreateMarket()));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlackScholesInputValidator.Validate(CreateOption(timeToMaturity: -1.0), CreateMarket()));
    }

    private static EuropeanOption CreateOption(double strike = 100.0, double timeToMaturity = 1.0) =>
        new(strike, timeToMaturity, OptionType.Call);

    private static MarketData CreateMarket(double spot = 100.0, double volatility = 0.20) =>
        new(spot, riskFreeRate: 0.05, volatility, dividendYield: 0.0);
}
