using NextStop.Pricing.BlackScholes;

namespace NextStop.Pricing.Tests.BlackScholes;

public class BlackScholesMathTests
{
    [Fact]
    public void GetD1_ShouldMatchKnownValue()
    {
        double d1 = BlackScholesMath.GetD1(
            s: 100.0,
            k: 100.0,
            r: 0.05,
            vol: 0.20,
            tau: 1.0,
            y: 0.0);

        Assert.Equal(0.35, d1, precision: 12);
    }

    [Fact]
    public void GetD2_ShouldMatchKnownValue()
    {
        double d2 = BlackScholesMath.GetD2(d1: 0.35, vol: 0.20, tau: 1.0);

        Assert.Equal(0.15, d2, precision: 12);
    }
}
