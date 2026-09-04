using MathNet.Numerics.LinearRegression;

namespace NextStop.Pricing.MonteCarlo.Regression;

internal sealed class ContinuationValueRegressor
{
    public const int BasisFunctionCount = 3;

    public QuadraticRegressionModel Fit(
        IReadOnlyList<double> spots,
        IReadOnlyList<double> continuationValues)
    {
        if (spots.Count != continuationValues.Count)
        {
            throw new ArgumentException("Spots and continuation values must have the same length.");
        }

        if (spots.Count < BasisFunctionCount)
        {
            throw new ArgumentException(
                $"At least {BasisFunctionCount} observations are required for quadratic regression.",
                nameof(spots));
        }

        double center = spots.Average();
        double scale = spots.Max(spot => Math.Abs(spot - center));

        if (scale <= 1e-12 * Math.Max(1.0, Math.Abs(center)))
        {
            return QuadraticRegressionModel.Constant(continuationValues.Average());
        }

        double[][] basisValues = spots
            .Select(spot =>
            {
                double normalizedSpot = (spot - center) / scale;
                return new[] { 1.0, normalizedSpot, normalizedSpot * normalizedSpot };
            })
            .ToArray();

        double[] normalizedCoefficients = MultipleRegression.NormalEquations(
            basisValues,
            continuationValues.ToArray(),
            intercept: false);

        return new QuadraticRegressionModel(normalizedCoefficients, center, scale);
    }
}

internal sealed class QuadraticRegressionModel
{
    private readonly IReadOnlyList<double> _normalizedCoefficients;
    private readonly double _center;
    private readonly double _scale;

    public double Intercept { get; }
    public double LinearCoefficient { get; }
    public double QuadraticCoefficient { get; }

    public QuadraticRegressionModel(
        IReadOnlyList<double> normalizedCoefficients,
        double center,
        double scale)
    {
        _normalizedCoefficients = normalizedCoefficients;
        _center = center;
        _scale = scale;

        QuadraticCoefficient = normalizedCoefficients[2] / (scale * scale);
        LinearCoefficient = normalizedCoefficients[1] / scale - 2.0 * center * QuadraticCoefficient;
        Intercept = normalizedCoefficients[0]
                    - normalizedCoefficients[1] * center / scale
                    + normalizedCoefficients[2] * center * center / (scale * scale);
    }

    public double Predict(double spot)
    {
        double normalizedSpot = (spot - _center) / _scale;
        return _normalizedCoefficients[0]
               + _normalizedCoefficients[1] * normalizedSpot
               + _normalizedCoefficients[2] * normalizedSpot * normalizedSpot;
    }

    public static QuadraticRegressionModel Constant(double value) =>
        new([value, 0.0, 0.0], center: 0.0, scale: 1.0);
}
