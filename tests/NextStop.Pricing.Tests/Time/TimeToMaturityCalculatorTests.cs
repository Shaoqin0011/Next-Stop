using NextStop.Domain.Time;

namespace NextStop.Pricing.Tests.Time;

public class TimeToMaturityCalculatorTests
{
    [Fact]
    public void OneActual365Year_ShouldReturnOne()
    {
        var valuation = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset maturity = valuation.AddDays(365);

        double tau = TimeToMaturityCalculator.Calculate(valuation, maturity);

        Assert.Equal(1.0, tau, precision: 12);
    }

    [Fact]
    public void Calculation_ShouldUseMinutePrecision()
    {
        var valuation = new DateTimeOffset(2026, 1, 1, 12, 0, 59, TimeSpan.Zero);
        var maturity = new DateTimeOffset(2026, 1, 1, 12, 2, 1, TimeSpan.Zero);

        double tau = TimeToMaturityCalculator.Calculate(valuation, maturity);

        Assert.Equal(2.0 / (365.0 * 24.0 * 60.0), tau, precision: 15);
    }

    [Fact]
    public void Calculation_ShouldCompareAbsoluteTimesAcrossOffsets()
    {
        var valuation = new DateTimeOffset(2026, 3, 8, 1, 0, 0, TimeSpan.FromHours(-5));
        var maturity = new DateTimeOffset(2026, 3, 8, 3, 0, 0, TimeSpan.FromHours(-4));

        double tau = TimeToMaturityCalculator.Calculate(valuation, maturity);

        Assert.Equal(60.0 / (365.0 * 24.0 * 60.0), tau, precision: 15);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonFutureMaturity_ShouldThrow(int minuteOffset)
    {
        var valuation = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset maturity = valuation.AddMinutes(minuteOffset);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TimeToMaturityCalculator.Calculate(valuation, maturity));
    }
}
