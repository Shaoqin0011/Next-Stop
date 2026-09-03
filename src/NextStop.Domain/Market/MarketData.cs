namespace NextStop.Domain.Market;

public sealed class MarketData
{
    public double Spot { get; }
    public double RiskFreeRate { get; }
    public double Volatility { get; }
    public double DividendYield { get; }

    public MarketData(
        double spot,
        double riskFreeRate,
        double volatility,
        double dividendYield)
    {
        Spot = spot;
        RiskFreeRate = riskFreeRate;
        Volatility = volatility;
        DividendYield = dividendYield;
    }
}