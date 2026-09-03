namespace NextStop.Domain.Instruments;

public sealed class EuropeanOption : Option
{
    public EuropeanOption(
        double strike,
        double timeToMaturity,
        OptionType type)
        : base(strike, timeToMaturity, type)
    {
    }
}