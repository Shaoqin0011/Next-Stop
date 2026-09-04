namespace NextStop.Domain.Time;

public static class TimeToMaturityCalculator
{
    private const double MinutesPerYear = 365.0 * 24.0 * 60.0;

    public static double Calculate(
        DateTimeOffset valuationTime,
        DateTimeOffset maturityTime)
    {
        DateTimeOffset valuationMinute = TruncateToMinute(valuationTime);
        DateTimeOffset maturityMinute = TruncateToMinute(maturityTime);
        double remainingMinutes = (maturityMinute - valuationMinute).TotalMinutes;

        if (remainingMinutes <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maturityTime),
                "Maturity must be later than the current time.");
        }

        return remainingMinutes / MinutesPerYear;
    }

    private static DateTimeOffset TruncateToMinute(DateTimeOffset value) =>
        new(
            value.Year,
            value.Month,
            value.Day,
            value.Hour,
            value.Minute,
            0,
            value.Offset);
}
