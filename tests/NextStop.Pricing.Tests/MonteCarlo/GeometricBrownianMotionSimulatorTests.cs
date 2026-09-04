using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Simulation;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class GeometricBrownianMotionSimulatorTests
{
    private readonly GeometricBrownianMotionSimulator _simulator = new();

    [Fact]
    public void SimulateTerminalPrice_WithZeroShock_ShouldMatchGbmFormula()
    {
        MarketData market = CreateMarket();
        const double timeToMaturity = 1.0;
        double expected = market.Spot * Math.Exp(
            (market.RiskFreeRate - market.DividendYield - 0.5 * market.Volatility * market.Volatility)
            * timeToMaturity);

        double actual = _simulator.SimulateTerminalPrice(market, timeToMaturity, z: 0.0);

        Assert.Equal(expected, actual, precision: 12);
    }

    [Fact]
    public void SimulateTerminalPrice_ShouldIncreaseWhenShockIncreases()
    {
        MarketData market = CreateMarket();

        double lowerShockPrice = _simulator.SimulateTerminalPrice(market, timeToMaturity: 1.0, z: -1.0);
        double higherShockPrice = _simulator.SimulateTerminalPrice(market, timeToMaturity: 1.0, z: 1.0);

        Assert.True(higherShockPrice > lowerShockPrice);
    }

    [Fact]
    public void SimulateTerminalPrice_ShouldAlwaysBePositive()
    {
        MarketData market = CreateMarket();
        double[] shocks = [-10.0, -1.0, 0.0, 1.0, 10.0];

        foreach (double shock in shocks)
        {
            double terminalPrice = _simulator.SimulateTerminalPrice(market, timeToMaturity: 1.0, shock);

            Assert.True(terminalPrice > 0.0);
        }
    }

    private static MarketData CreateMarket() =>
        new(spot: 100.0, riskFreeRate: 0.05, volatility: 0.20, dividendYield: 0.0);
}
