using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Setting;

public static class Program
{
    public static void Main()
    {
        var option = new AmericanOption(
            strike: 100.0,
            timeToMaturity: 1.0,
            type: OptionType.Put);

        var market = new MarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20,
            dividendYield: 0.0);

        var settings = new PathMonteCarloSettings(
            numberOfPaths: 5,
            numberOfTimeSteps: 3,
            randomSeed: 42);

        var engine = new MonteCarloPricingEngine();

        double price = engine.PriceWithPath(
            option,
            market,
            settings);

        Console.WriteLine($"American Option Price: {price}");
    }
}