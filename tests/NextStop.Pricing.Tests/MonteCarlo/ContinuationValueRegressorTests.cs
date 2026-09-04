using NextStop.Pricing.MonteCarlo.Regression;

namespace NextStop.Pricing.Tests.MonteCarlo;

public class ContinuationValueRegressorTests
{
    private readonly ContinuationValueRegressor _regressor = new();

    [Fact]
    public void Fit_WithExactQuadraticData_ShouldRecoverCoefficients()
    {
        double[] spots = [1.0, 2.0, 3.0, 4.0, 5.0];
        double[] continuationValues = spots
            .Select(spot => 2.0 + 3.0 * spot + 4.0 * spot * spot)
            .ToArray();

        QuadraticRegressionModel regression = _regressor.Fit(spots, continuationValues);

        Assert.Equal(2.0, regression.Intercept, precision: 8);
        Assert.Equal(3.0, regression.LinearCoefficient, precision: 8);
        Assert.Equal(4.0, regression.QuadraticCoefficient, precision: 8);
    }

    [Fact]
    public void Fit_WithNoisyQuadraticData_ShouldProduceReasonablePrediction()
    {
        double[] spots = [1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0];
        double[] noise = [-0.40, 0.20, 0.10, -0.30, 0.25, -0.15, 0.35, -0.20, 0.15, 0.05];
        double[] continuationValues = spots
            .Select((spot, index) => 2.0 + 3.0 * spot + 4.0 * spot * spot + noise[index])
            .ToArray();

        QuadraticRegressionModel regression = _regressor.Fit(spots, continuationValues);
        double prediction = regression.Predict(spot: 6.5);
        double expectedWithoutNoise = 2.0 + 3.0 * 6.5 + 4.0 * 6.5 * 6.5;

        Assert.InRange(prediction, expectedWithoutNoise - 0.50, expectedWithoutNoise + 0.50);
    }
}
