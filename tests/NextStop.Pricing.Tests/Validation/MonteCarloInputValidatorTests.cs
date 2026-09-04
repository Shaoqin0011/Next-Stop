using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Setting;
using NextStop.Pricing.MonteCarlo.Validation;

namespace NextStop.Pricing.Tests.Validation;

public class MonteCarloInputValidatorTests
{
    private static readonly MonteCarloSettings TerminalSettings = new(numberOfPaths: 10, randomSeed: 42);
    private static readonly PathMonteCarloSettings PathSettings = new(numberOfPaths: 10, numberOfTimeSteps: 5, randomSeed: 42);

    [Fact]
    public void TerminalPriceValidate_ShouldRejectDividendPayingAmericanOption()
    {
        var option = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);
        MarketData market = CreateMarket(dividendYield: 0.02);

        NotSupportedException exception = Assert.Throws<NotSupportedException>(
            () => TerminalPriceInputValidator.Validate(option, market, TerminalSettings));

        Assert.Contains("dividend-paying American options", exception.Message);
    }

    [Fact]
    public void TerminalPriceValidate_ShouldAcceptNonDividendPayingAmericanOption()
    {
        var option = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);

        TerminalPriceInputValidator.Validate(option, CreateMarket(), TerminalSettings);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void BothValidators_ShouldRejectNonPositiveStrike(double strike)
    {
        var option = new EuropeanOption(strike, timeToMaturity: 1.0, OptionType.Call);
        MarketData market = CreateMarket();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalPriceInputValidator.Validate(option, market, TerminalSettings));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PathInputValidator.Validate(option, market, PathSettings));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void BothValidators_ShouldRejectNonPositiveTimeToMaturity(double timeToMaturity)
    {
        var option = new EuropeanOption(strike: 100.0, timeToMaturity, OptionType.Call);
        MarketData market = CreateMarket();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalPriceInputValidator.Validate(option, market, TerminalSettings));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PathInputValidator.Validate(option, market, PathSettings));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void BothValidators_ShouldRejectNonPositiveSpot(double spot)
    {
        EuropeanOption option = CreateOption();
        MarketData market = CreateMarket(spot: spot);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalPriceInputValidator.Validate(option, market, TerminalSettings));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PathInputValidator.Validate(option, market, PathSettings));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.01)]
    public void BothValidators_ShouldRejectNonPositiveVolatility(double volatility)
    {
        EuropeanOption option = CreateOption();
        MarketData market = CreateMarket(volatility: volatility);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalPriceInputValidator.Validate(option, market, TerminalSettings));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PathInputValidator.Validate(option, market, PathSettings));
    }

    [Fact]
    public void PriceWithTerminalPrice_ShouldApplyTerminalPriceValidation()
    {
        var option = new AmericanOption(strike: 100.0, timeToMaturity: 1.0, OptionType.Put);
        MarketData market = CreateMarket(dividendYield: 0.01);

        Assert.Throws<NotSupportedException>(
            () => new global::MonteCarloPricingEngine().PriceWithTerminalPrice(option, market, TerminalSettings));
    }

    private static EuropeanOption CreateOption() =>
        new(strike: 100.0, timeToMaturity: 1.0, OptionType.Call);

    private static MarketData CreateMarket(
        double spot = 100.0,
        double volatility = 0.20,
        double dividendYield = 0.0) =>
        new(spot, riskFreeRate: 0.05, volatility, dividendYield);
}
