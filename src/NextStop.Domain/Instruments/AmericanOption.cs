namespace NextStop.Domain.Instruments;

public sealed class AmericanOption : Option
{
    public AmericanOption(
        double strike,
        double timeToMaturity,
        OptionType type)
        : base(strike, timeToMaturity, type)
    {
    }
}