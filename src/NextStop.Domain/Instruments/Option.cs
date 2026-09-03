namespace NextStop.Domain.Instruments;

public abstract class Option
{
    public double Strike { get; }
    public double TimeToMaturity { get; }
    public OptionType Type { get; }

    protected Option(
        double strike,
        double timeToMaturity,
        OptionType type)
    {
        Strike = strike;
        TimeToMaturity = timeToMaturity;
        Type = type;
    }

    public double Payoff(double spot)
    {
        if (Type == OptionType.Call)
        {
            return Math.Max(spot - Strike, 0.0);
        }

        return Math.Max(Strike - spot, 0.0);
    }

    public Boolean IsInTheMoney(double spot)
    {
        if (Type == OptionType.Call)
        {
            return spot > Strike;
        }

        return spot < Strike;
    }
}