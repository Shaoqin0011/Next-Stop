using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.MonteCarlo.Random;
using NextStop.Pricing.MonteCarlo.Simulation;
using NextStop.Pricing.MonteCarlo.Setting;
using MathNet.Numerics.LinearRegression;

public sealed class MonteCarloPricingEngine
{
    public double PriceWithPathTerminal(
        Option option,
        MarketData market,
        PathMonteCarloSettings settings)
    {
        var randomGenerator = new GaussianRandomGenerator(settings.RandomSeed);
        var pathSimulator = new GeometricBrownianMotionPathSimulator();
        double sumPayoffs = 0.0;

        for (int i = 0; i < settings.NumberOfPaths; i++)
        {
            double[] shocks = new double[settings.NumberOfTimeSteps];
            randomGenerator.Fill(shocks);

            double[] path = pathSimulator.SimulatePath(market, option.TimeToMaturity, shocks);
            sumPayoffs += option.Payoff(path[^1]);
        }

        double averagePayoff = sumPayoffs / settings.NumberOfPaths;
        return averagePayoff * Math.Exp(-market.RiskFreeRate * option.TimeToMaturity);
    }

    public double PriceWithTerminalPrice(
        Option option,
        MarketData market,
        MonteCarloSettings settings)
    {
        var randomGenerator = new GaussianRandomGenerator(settings.RandomSeed);
        var terminalPriceSimulator = new GeometricBrownianMotionSimulator();

        double sumPayoffs = 0.0;

        for (int i = 0; i < settings.NumberOfPaths; i++)
        {
            double z = randomGenerator.Next();
            double terminalPrice = terminalPriceSimulator.SimulateTerminalPrice(market, option.TimeToMaturity, z);
            double payoff = option.Type == OptionType.Call
                ? Math.Max(terminalPrice - option.Strike, 0)
                : Math.Max(option.Strike - terminalPrice, 0);

            sumPayoffs += payoff;
        }

        double averagePayoff = sumPayoffs / settings.NumberOfPaths;
        double discountedPayoff = averagePayoff * Math.Exp(-market.RiskFreeRate * option.TimeToMaturity);

        return discountedPayoff;
    }

    public double PriceWithPath(
        Option option,
        MarketData market,
        PathMonteCarloSettings settings)
    {
        int numberOfPaths = settings.NumberOfPaths;
        int numberOfSteps = settings.NumberOfTimeSteps;
        var randomGenerator = new GaussianRandomGenerator(settings.RandomSeed);
        var pathSimulator = new GeometricBrownianMotionPathSimulator();
        double[][] paths = new double[numberOfPaths][];

        double r = market.RiskFreeRate;
        double tau = option.TimeToMaturity;
        
        // Path Generation
        for (int i = 0; i < numberOfPaths; i++)
        {
            double[] zSeries = new double[numberOfSteps];
            randomGenerator.Fill(zSeries);

            paths[i] = pathSimulator.SimulatePath(market, tau, zSeries);
        }
        
        double dt = option.TimeToMaturity / numberOfSteps;
        double stepDiscountTerm = Math.Exp(-r * dt);
        double[] immediateExerciseValues = new double[numberOfPaths];
        double[] expectedContinuationValues = new double[numberOfPaths];
        double[] optimalMovementValues = new double[numberOfPaths];
        for (int i = numberOfSteps; i >= 1; i--)
        {   
            // immediateExerciseValues and expectedContinuationValues calculation
            double[] spots = new double[numberOfPaths];
            for (int j = 0; j < numberOfPaths; j++)
            {   
                double currentPrice = paths[j][i];
                spots[j] = currentPrice;
                double immediateExerciseValue = option.Payoff(currentPrice);
                if (i == numberOfSteps)
                {
                    optimalMovementValues[j] = immediateExerciseValue;
                    continue;
                }

                immediateExerciseValues[j] = immediateExerciseValue;
                expectedContinuationValues[j] = optimalMovementValues[j] * stepDiscountTerm;
            }
            if (i == numberOfSteps) continue;

            // Longstaff-Schwartz regression for continuation value estimation
            var regressionXs = new List<double[]>();
            var regressionYs = new List<double>();
            for (int j = 0; j < spots.Length; j++)
            {
                double spot = spots[j];
                if (!option.IsInTheMoney(spot)) continue;
                
                regressionXs.Add([1.0, spot, spot * spot]);
                regressionYs.Add(expectedContinuationValues[j]);
            }
            if (regressionXs.Count < 3)
            {
                for (int j = 0; j < spots.Length; j++)
                {
                    optimalMovementValues[j] = expectedContinuationValues[j];
                }

                continue;
            }
            double[] beta = MultipleRegression.NormalEquations([.. regressionXs], 
                                                               [.. regressionYs], 
                                                               intercept: false);

            for (int j = 0; j < spots.Length; j++)
            {
                double spot = spots[j];
                if (!option.IsInTheMoney(spot))
                {
                    optimalMovementValues[j] = expectedContinuationValues[j];
                    continue;
                }
                
                double continuationValue = beta[0] + beta[1] * spot + beta[2] * spot * spot;
                if (immediateExerciseValues[j] > continuationValue)
                {
                    optimalMovementValues[j] = immediateExerciseValues[j];
                }
                else
                {
                    optimalMovementValues[j] = expectedContinuationValues[j];
                }
            }
        }

        double averageOptimalValue = optimalMovementValues.Sum() / optimalMovementValues.Length;
        double continuationValueAtValuation = averageOptimalValue * stepDiscountTerm;
        double intrinsicValueAtValuation = option.Payoff(market.Spot);

        return Math.Max(continuationValueAtValuation, intrinsicValueAtValuation);
    }
}

