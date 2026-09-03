using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Random;
using NextStop.Pricing.MonteCarlo.Setting;

namespace NextStop.Pricing.MonteCarlo.Simulation;


public sealed class GeometricBrownianMotionPathSimulator
{
    public double[] SimulatePath(
        MarketData market,
        double timeToMaturity,
        double[] zSeries)
    {   
        int numberOfSteps = zSeries.Length;
        double dt = timeToMaturity / numberOfSteps;
        
        double r = market.RiskFreeRate;
        double y = market.DividendYield;
        double vol = market.Volatility;

        double[] path = new double[numberOfSteps + 1];
        path[0] = market.Spot;
        for(int i = 0; i < numberOfSteps; i++)
        {
            double currentPrice = path[i];

            double drift = (r - y - 0.5 * vol * vol) * dt;
            double diffusion = vol * Math.Sqrt(dt) * zSeries[i];

            path[i + 1] = currentPrice * Math.Exp(drift + diffusion);
        }

        return path;
    }






}



