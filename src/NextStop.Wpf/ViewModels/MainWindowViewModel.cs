using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using NextStop.Domain.Instruments;
using NextStop.Domain.Market;
using NextStop.Domain.Time;
using NextStop.Pricing.BlackScholes;
using NextStop.Pricing.MonteCarlo;
using NextStop.Pricing.MonteCarlo.Setting;
using NextStop.Pricing.Results;

namespace NextStop.Wpf.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly TimeProvider _timeProvider;
    private readonly DispatcherTimer _localClockTimer;
    private string _selectedExerciseStyle = "European";
    private string _selectedOptionType = "Call";
    private string _selectedPricingEngine = "Black-Scholes";
    private string _selectedMonteCarloMethodology = "Path";
    private string _selectedTimeInputMethod = "Tau (years)";
    private string _spotText = "100";
    private string _strikeText = "100";
    private string _timeToMaturityText = "1";
    private DateTime? _maturityDate;
    private string _maturityTimeText = "16:00";
    private string _localDateTimeDisplay = string.Empty;
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

    public MainWindowViewModel(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _maturityDate = _timeProvider.GetLocalNow().Date.AddYears(1);
        UpdateLocalDateTime();
        _localClockTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _localClockTimer.Tick += (_, _) => UpdateLocalDateTime();
        _localClockTimer.Start();
        PriceCommand = new RelayCommand(CalculatePrice);
        ResetCommand = new RelayCommand(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> ExerciseStyles { get; } = ["European", "American"];
    public IReadOnlyList<string> OptionTypes { get; } = ["Call", "Put"];
    public IReadOnlyList<string> PricingEngines { get; } = ["Black-Scholes", "Monte Carlo"];
    public IReadOnlyList<string> MonteCarloMethodologies { get; } = ["Path", "Terminal Price"];
    public IReadOnlyList<string> TimeInputMethods { get; } = ["Tau (years)", "Maturity date"];
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

    public string SelectedTimeInputMethod
    {
        get => _selectedTimeInputMethod;
        set
        {
            if (!SetField(ref _selectedTimeInputMethod, value))
            {
                return;
            }

            OnPropertyChanged(nameof(UsesManualTau));
            OnPropertyChanged(nameof(UsesMaturityDate));
        }
    }

    public string SpotText { get => _spotText; set => SetField(ref _spotText, value); }
    public string StrikeText { get => _strikeText; set => SetField(ref _strikeText, value); }
    public string TimeToMaturityText { get => _timeToMaturityText; set => SetField(ref _timeToMaturityText, value); }
    public DateTime? MaturityDate { get => _maturityDate; set => SetField(ref _maturityDate, value); }
    public string MaturityTimeText { get => _maturityTimeText; set => SetField(ref _maturityTimeText, value); }
    public string LocalDateTimeDisplay { get => _localDateTimeDisplay; private set => SetField(ref _localDateTimeDisplay, value); }
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
    public bool UsesManualTau => SelectedTimeInputMethod == "Tau (years)";
    public bool UsesMaturityDate => !UsesManualTau;
    public string ResultDescription => IsMonteCarlo
        ? $"Monte Carlo {SelectedMonteCarloMethodology.ToLowerInvariant()} result with a fixed random seed."
        : "Black–Scholes result and analytical Greeks.";

    private void CalculatePrice()
    {
        try
        {
            var market = new MarketData(
                spot: ReadNumber(SpotText, "Spot price"),
                riskFreeRate: ReadNumber(RiskFreeRatePercentText, "Risk-free rate") / 100.0,
                volatility: ReadNumber(VolatilityPercentText, "Volatility") / 100.0,
                dividendYield: ReadNumber(DividendYieldPercentText, "Dividend yield") / 100.0);

            Option option = CreateOption();

            if (!IsMonteCarlo)
            {
                PriceWithBlackScholes(option, market);
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
        double strike = ReadNumber(StrikeText, "Strike price");
        double tau = ResolveTimeToMaturity();
        OptionType type = SelectedOptionType == "Put" ? OptionType.Put : OptionType.Call;

        if (SelectedExerciseStyle == "American")
        {
            return new AmericanOption(strike, tau, type);
        }

        return new EuropeanOption(strike, tau, type);
    }

    private double ResolveTimeToMaturity()
    {
        double tau;

        if (UsesManualTau)
        {
            tau = ReadNumber(TimeToMaturityText, "time to maturity");
        }
        else
        {
            if (MaturityDate is null)
            {
                throw new ArgumentException("Select a maturity date.");
            }

            if (!DateTime.TryParseExact(
                    MaturityTimeText,
                    ["H:mm", "HH:mm"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedTime))
            {
                throw new ArgumentException("Enter maturity time as HH:mm.");
            }

            DateTime localMaturity = DateTime.SpecifyKind(
                MaturityDate.Value.Date + parsedTime.TimeOfDay,
                DateTimeKind.Unspecified);

            if (TimeZoneInfo.Local.IsInvalidTime(localMaturity))
            {
                throw new ArgumentException("The selected maturity time does not exist in the local time zone.");
            }

            var maturity = new DateTimeOffset(
                localMaturity,
                TimeZoneInfo.Local.GetUtcOffset(localMaturity));

            tau = TimeToMaturityCalculator.Calculate(
                _timeProvider.GetLocalNow(),
                maturity);
        }

        return tau;
    }

    private void UpdateLocalDateTime()
    {
        LocalDateTimeDisplay = _timeProvider
            .GetLocalNow()
            .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    private void PriceWithBlackScholes(Option option, MarketData market)
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
        int numberOfPaths = ReadInteger(NumberOfPathsText, "Simulation paths");
        var engine = new MonteCarloPricingEngine();

        if (!UsesPathMonteCarlo)
        {
            var terminalSettings = new MonteCarloSettings(numberOfPaths, randomSeed: 42);
            SetMonteCarloResult(engine.PriceWithTerminalPrice(option, market, terminalSettings));
            return;
        }

        var settings = new PathMonteCarloSettings(
            numberOfPaths,
            ReadInteger(NumberOfTimeStepsText, "Time steps"),
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
        SelectedTimeInputMethod = "Tau (years)";
        SpotText = StrikeText = "100";
        TimeToMaturityText = "1";
        MaturityDate = _timeProvider.GetLocalNow().Date.AddYears(1);
        MaturityTimeText = "16:00";
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

    private static int ReadInteger(string value, string name)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out int parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Enter a valid whole number for {name}.");
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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
