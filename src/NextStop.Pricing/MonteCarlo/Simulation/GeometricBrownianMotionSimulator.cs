using NextStop.Domain.Market;

namespace NextStop.Pricing.MonteCarlo.Simulation;

public sealed class GeometricBrownianMotionSimulator
{
    public double SimulateTerminalPrice(
        MarketData market,
        double timeToMaturity,
        double z)
    {
        double s = market.Spot;
        double r = market.RiskFreeRate;
        double y = market.DividendYield;
        double vol = market.Volatility;
        double tau = timeToMaturity;

        double drift = (r - y - 0.5 * vol * vol) * tau;
        double diffusion = vol * Math.Sqrt(tau) * z;

        return s * Math.Exp(drift + diffusion);
    }
}