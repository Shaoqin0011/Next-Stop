using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Simulation;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class GeometricBrownianMotionPathSimulatorTests
{
    private readonly GeometricBrownianMotionPathSimulator _simulator = new();

    [Fact]
    public void SimulatePath_LengthShouldEqualNumberOfStepsPlusOne()
    {
        double[] shocks = [0.0, 0.5, -0.25, 1.0];

        double[] path = _simulator.SimulatePath(CreateMarket(), timeToMaturity: 1.0, shocks);

        Assert.Equal(shocks.Length + 1, path.Length);
    }

    [Fact]
    public void SimulatePath_FirstPriceShouldEqualSpot()
    {
        MarketData market = CreateMarket();

        double[] path = _simulator.SimulatePath(market, timeToMaturity: 1.0, [0.0, 0.5]);

        Assert.Equal(market.Spot, path[0]);
    }

    [Fact]
    public void SimulatePath_WithFixedShocksShouldBeDeterministic()
    {
        MarketData market = CreateMarket();
        double[] shocks = [0.0, 0.5, -0.25, 1.0];

        double[] firstPath = _simulator.SimulatePath(market, timeToMaturity: 1.0, shocks);
        double[] secondPath = _simulator.SimulatePath(market, timeToMaturity: 1.0, shocks);

        Assert.Equal(firstPath, secondPath);
    }

    [Fact]
    public void SimulatePath_AllPricesShouldBePositive()
    {
        double[] shocks = [-2.0, -1.0, 0.0, 1.0, 2.0];

        double[] path = _simulator.SimulatePath(CreateMarket(), timeToMaturity: 1.0, shocks);

        Assert.All(path, price => Assert.True(price > 0.0));
    }

    private static MarketData CreateMarket() =>
        new(spot: 100.0, riskFreeRate: 0.05, volatility: 0.20, dividendYield: 0.0);
}
