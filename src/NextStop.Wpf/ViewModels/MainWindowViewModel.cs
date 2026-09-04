using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Pricing.BlackScholes;
using NextStop.Pricing.MonteCarlo;
using NextStop.Pricing.MonteCarlo.Setting;
using NextStop.Pricing.Results;

namespace NextStop.Wpf.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private string _selectedExerciseStyle = "European";
    private string _selectedOptionType = "Call";
    private string _selectedPricingEngine = "Black-Scholes";
    private string _selectedMonteCarloMethodology = "Path";
    private string _spotText = "100";
    private string _strikeText = "100";
    private string _timeToMaturityText = "1";
    private string _volatilityPercentText = "20";
    private string _riskFreeRatePercentText = "5";
    private string _dividendYieldPercentText = "0";
    private string _numberOfPathsText = "10000";
    private string _numberOfTimeStepsText = "50";
    private string _errorMessage = string.Empty;
    private string _priceDisplay = "—";
    private string _deltaDisplay = "—";
    private string _gammaDisplay = "—";
    private string _thetaDisplay = "—";
    private string _vegaDisplay = "—";
    private string _rhoDisplay = "—";

    public MainWindowViewModel()
    {
        PriceCommand = new RelayCommand(CalculatePrice);
        ResetCommand = new RelayCommand(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> ExerciseStyles { get; } = ["European", "American"];
    public IReadOnlyList<string> OptionTypes { get; } = ["Call", "Put"];
    public IReadOnlyList<string> PricingEngines { get; } = ["Black-Scholes", "Monte Carlo"];
    public IReadOnlyList<string> MonteCarloMethodologies { get; } = ["Path", "Terminal Price"];
    public ICommand PriceCommand { get; }
    public ICommand ResetCommand { get; }

    public string SelectedExerciseStyle
    {
        get => _selectedExerciseStyle;
        set
        {
            SetField(ref _selectedExerciseStyle, value);
            NotifyPricingSelectionChanged();
        }
    }

    public string SelectedOptionType { get => _selectedOptionType; set => SetField(ref _selectedOptionType, value); }

    public string SelectedPricingEngine
    {
        get => _selectedPricingEngine;
        set
        {
            SetField(ref _selectedPricingEngine, value);
            NotifyPricingSelectionChanged();
        }
    }

    public string SelectedMonteCarloMethodology
    {
        get => _selectedMonteCarloMethodology;
        set
        {
            SetField(ref _selectedMonteCarloMethodology, value);
            NotifyPricingSelectionChanged();
        }
    }

    public string SpotText { get => _spotText; set => SetField(ref _spotText, value); }
    public string StrikeText { get => _strikeText; set => SetField(ref _strikeText, value); }
    public string TimeToMaturityText { get => _timeToMaturityText; set => SetField(ref _timeToMaturityText, value); }
    public string VolatilityPercentText { get => _volatilityPercentText; set => SetField(ref _volatilityPercentText, value); }
    public string RiskFreeRatePercentText { get => _riskFreeRatePercentText; set => SetField(ref _riskFreeRatePercentText, value); }
    public string DividendYieldPercentText { get => _dividendYieldPercentText; set => SetField(ref _dividendYieldPercentText, value); }
    public string NumberOfPathsText { get => _numberOfPathsText; set => SetField(ref _numberOfPathsText, value); }
    public string NumberOfTimeStepsText { get => _numberOfTimeStepsText; set => SetField(ref _numberOfTimeStepsText, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetField(ref _errorMessage, value); }
    public string PriceDisplay { get => _priceDisplay; private set => SetField(ref _priceDisplay, value); }
    public string DeltaDisplay { get => _deltaDisplay; private set => SetField(ref _deltaDisplay, value); }
    public string GammaDisplay { get => _gammaDisplay; private set => SetField(ref _gammaDisplay, value); }
    public string ThetaDisplay { get => _thetaDisplay; private set => SetField(ref _thetaDisplay, value); }
    public string VegaDisplay { get => _vegaDisplay; private set => SetField(ref _vegaDisplay, value); }
    public string RhoDisplay { get => _rhoDisplay; private set => SetField(ref _rhoDisplay, value); }
    public bool IsMonteCarlo => SelectedPricingEngine == "Monte Carlo";
    public bool UsesPathMonteCarlo => IsMonteCarlo && SelectedMonteCarloMethodology == "Path";
    public string ResultDescription => IsMonteCarlo
        ? $"Monte Carlo {SelectedMonteCarloMethodology.ToLowerInvariant()} result with a fixed random seed."
        : "Black–Scholes result and analytical Greeks.";

    private void CalculatePrice()
    {
        try
        {
            var market = new MarketData(
                spot: ReadPositive(SpotText, "Spot price"),
                riskFreeRate: ReadNumber(RiskFreeRatePercentText, "Risk-free rate") / 100.0,
                volatility: ReadPositive(VolatilityPercentText, "Volatility") / 100.0,
                dividendYield: ReadNumber(DividendYieldPercentText, "Dividend yield") / 100.0);

            Option option = CreateOption();

            if (!IsMonteCarlo)
            {
                if (option is not EuropeanOption europeanOption)
                {
                    throw new NotSupportedException("Black-Scholes is available for European options only. Select Monte Carlo for American options.");
                }

                PriceWithBlackScholes(europeanOption, market);
                return;
            }

            PriceWithMonteCarlo(option, market);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or NotSupportedException)
        {
            ErrorMessage = exception.Message;
        }
    }

    private Option CreateOption()
    {
        double strike = ReadPositive(StrikeText, "Strike price");
        double maturity = ReadPositive(TimeToMaturityText, "Time to maturity");
        OptionType type = SelectedOptionType == "Put" ? OptionType.Put : OptionType.Call;

        if (SelectedExerciseStyle == "American")
        {
            return new AmericanOption(strike, maturity, type);
        }

        return new EuropeanOption(strike, maturity, type);
    }

    private void PriceWithBlackScholes(EuropeanOption option, MarketData market)
    {
        double price = new BlackScholesPricingEngine().Price(option, market);
        Greeks greeks = new BlackScholesGreeksCalculator().CalculateAll(option, market);

        PriceDisplay = Format(price);
        DeltaDisplay = Format(greeks.Delta);
        GammaDisplay = Format(greeks.Gamma);
        ThetaDisplay = Format(greeks.Theta);
        VegaDisplay = Format(greeks.Vega);
        RhoDisplay = Format(greeks.Rho);
        ErrorMessage = string.Empty;
    }

    private void PriceWithMonteCarlo(Option option, MarketData market)
    {
        int numberOfPaths = ReadPositiveInteger(NumberOfPathsText, "Simulation paths");
        var engine = new MonteCarloPricingEngine();

        if (!UsesPathMonteCarlo)
        {
            var terminalSettings = new MonteCarloSettings(numberOfPaths, randomSeed: 42);
            SetMonteCarloResult(engine.PriceWithTerminalPrice(option, market, terminalSettings));
            return;
        }

        var settings = new PathMonteCarloSettings(
            numberOfPaths,
            ReadPositiveInteger(NumberOfTimeStepsText, "Time steps"),
            randomSeed: 42);
        double price = option is AmericanOption
            ? engine.PriceWithPath(option, market, settings)
            : engine.PriceWithPathTerminal(option, market, settings);

        SetMonteCarloResult(price);
    }

    private void SetMonteCarloResult(double price)
    {
        PriceDisplay = Format(price);
        DeltaDisplay = GammaDisplay = ThetaDisplay = VegaDisplay = RhoDisplay = "—";
        ErrorMessage = string.Empty;
    }

    private void Reset()
    {
        SelectedExerciseStyle = "European";
        SelectedOptionType = "Call";
        SelectedPricingEngine = "Black-Scholes";
        SelectedMonteCarloMethodology = "Path";
        SpotText = StrikeText = "100";
        TimeToMaturityText = "1";
        VolatilityPercentText = "20";
        RiskFreeRatePercentText = "5";
        DividendYieldPercentText = "0";
        NumberOfPathsText = "10000";
        NumberOfTimeStepsText = "50";
        PriceDisplay = DeltaDisplay = GammaDisplay = ThetaDisplay = VegaDisplay = RhoDisplay = "—";
        ErrorMessage = string.Empty;
    }

    private void NotifyPricingSelectionChanged()
    {
        OnPropertyChanged(nameof(IsMonteCarlo));
        OnPropertyChanged(nameof(UsesPathMonteCarlo));
        OnPropertyChanged(nameof(ResultDescription));
    }

    private static string Format(double value) => value.ToString("N4", CultureInfo.CurrentCulture);

    private static double ReadPositive(string value, string name)
    {
        double parsed = ReadNumber(value, name);
        if (parsed <= 0)
        {
            throw new ArgumentOutOfRangeException(name, $"{name} must be greater than zero.");
        }

        return parsed;
    }

    private static int ReadPositiveInteger(string value, string name)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out int parsed) && parsed > 0)
        {
            return parsed;
        }

        throw new ArgumentException($"{name} must be a positive whole number.");
    }

    private static double ReadNumber(string value, string name)
    {
        if ((double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out double parsed) ||
             double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)) &&
            double.IsFinite(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Enter a valid number for {name}.");
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
