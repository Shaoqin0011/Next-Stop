namespace NextStop.Pricing.Results;

public sealed record Greeks(
    double Delta,
    double Gamma,
    double Theta,
    double Vega,
    double Rho
);