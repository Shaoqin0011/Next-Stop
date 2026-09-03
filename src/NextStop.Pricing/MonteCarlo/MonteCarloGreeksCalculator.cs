// using NextStop.Domain.Instruments;
// using NextStop.Domain.Market;
// using NextStop.Pricing.MonteCarlo.Random;
// using NextStop.Pricing.MonteCarlo.Simulation;
// using NextStop.Pricing.MonteCarlo.Setting;
// using MathNet.Numerics.LinearRegression;

// public sealed class MonteCarloGreeksCalculator
// {
//     private readonly MonteCarloPricingEngine _pricingEngine;

//     public MonteCarloGreeksCalculator()
//     {
//         _pricingEngine = new MonteCarloPricingEngine();
//     }

//     public double CalculateDelta(Option option, 
//                                  MarketData market, 
//                                  PathMonteCarloSettings settings, 
//                                  double spotShift)
//     {
//         var marketUp = new MarketData(market.SpotPrice + spotShift, market.RiskFreeRate, market.DividendYield, market.Volatility);
//         var marketDown = new MarketData(market.SpotPrice - spotShift, market.RiskFreeRate, market.DividendYield, market.Volatility);

//         double priceUp = option is AmericanOption
//             ? _pricingEngine.PriceWithPath(option, marketUp, settings)
//             : _pricingEngine.PriceWithPathTerminal(option, marketUp, settings);

//         double priceDown = option is AmericanOption
//             ? _pricingEngine.PriceWithPath(option, marketDown, settings)
//             : _pricingEngine.PriceWithPathTerminal(option, marketDown, settings);

//         return (priceUp - priceDown) / (2 * spotShift);
//     }
// }