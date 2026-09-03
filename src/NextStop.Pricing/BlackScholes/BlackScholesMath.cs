namespace NextStop.Pricing.BlackScholes;

internal static class BlackScholesMath
{
    public static double GetD1(double s, double k, double r, double vol, double tau, double y)
    {
        return (
            Math.Log(s / k) + (r - y + 0.5 * vol * vol) * tau) / 
            (vol * Math.Sqrt(tau));
    }

    public static double GetD2(double d1, double vol, double tau)
    {
        return d1 - vol * Math.Sqrt(tau);
    }
}