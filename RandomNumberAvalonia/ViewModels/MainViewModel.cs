using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RandomNumberAvalonia.Core;
using RandomNumberAvalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RandomNumberAvalonia.ViewModels;

/// <summary>底部导航的三个页面。</summary>
public enum AppPage
{
    Home,
    History,
    Settings,
}

public partial class MainViewModel : ViewModelBase
{
    private readonly PeriodicTimer timer;
    private readonly AppSettings? settings;
    private readonly HistoryStore? history;
    private HistorySessionViewModel? currentSessionViewModel;
    private bool isLoading;
    private IRandomNumberGenerator? randomNumberGenerator;
    private CancellationTokenSource? cts;

    [ObservableProperty]
    private AppPage currentPage = AppPage.Home;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartStopButtonText))]
    private bool isRunning;

    public string StartStopButtonText => IsRunning ? "停止" : "开始";

    [ObservableProperty]
    private ObservableCollection<int> generatedRandomNumbers = [];

    [ObservableProperty]
    private ObservableCollection<HistorySessionViewModel> historySessions = [];

    public string HistorySummary => $"{HistorySessions.Count} 次启动 · 共 {HistorySessions.Sum(s => s.Entries.Count)} 个数字";

    public bool HasHistory => HistorySessions.Count > 0;

    [ObservableProperty]
    private int randomNumber = 0;

    [ObservableProperty]
    private int range = 10;

    partial void OnRangeChanged(int value) => SaveSettings();

    [ObservableProperty]
    [Range(0, int.MaxValue - 1)]
    private int minValue;

    partial void OnMinValueChanged(int value)
    {
        if (randomNumberGenerator is null)
        {
            return;
        }
        if (randomNumberGenerator is StrongRandomNumberGenerator)//强随机数生成器区间是(min,max);而弱随机数生成器区间是[min,max)
        {
            randomNumberGenerator.MinValue = value - 1;
        }
        else
        {
            randomNumberGenerator.MinValue = value;
        }
        Description = $"{MinValue} ≤ n < {MaxValue}";
        SaveSettings();
    }

    [ObservableProperty]
    [Range(1, int.MaxValue)]
    private int maxValue;

    partial void OnMaxValueChanged(int value)
    {
        if (randomNumberGenerator is null)
        {
            return;
        }
        randomNumberGenerator.MaxValue = value;
        Description = $"{MinValue} ≤ n < {MaxValue}";
        SaveSettings();
    }

    [ObservableProperty]
    private bool isUniqueEnabled = false;

    partial void OnIsUniqueEnabledChanged(bool value) => SaveSettings();

    [ObservableProperty]
    private bool isRangeEnabled = false;

    partial void OnIsRangeEnabledChanged(bool value) => SaveSettings();

    [ObservableProperty]
    private bool isStrongRandomNumberGenerator;

    partial void OnIsStrongRandomNumberGeneratorChanged(bool value)
    {
        randomNumberGenerator = value ? StrongRandomNumberGenerator.Instance : FakeRandomNumberGenerator.Instance;
        randomNumberGenerator.MinValue = value ? MinValue - 1 : MinValue;
        randomNumberGenerator.MaxValue = MaxValue;
        SaveSettings();
    }

    [ObservableProperty]
    string description = string.Empty;

    /// <summary>设计器与单元测试使用的构造函数：不做任何文件读写。</summary>
    public MainViewModel() : this(null, null)
    {
    }

    public MainViewModel(AppSettings? settings, HistoryStore? history)
    {
        isLoading = true;
        timer = new(TimeSpan.FromMilliseconds(1));
        this.settings = settings;
        this.history = history;

        MinValue = settings?.MinValue ?? 0;
        MaxValue = settings?.MaxValue ?? 100;
        Range = settings?.Range ?? 10;
        IsUniqueEnabled = settings?.IsUniqueEnabled ?? true;
        IsRangeEnabled = settings?.IsRangeEnabled ?? true;
        IsStrongRandomNumberGenerator = settings?.IsStrongRandomNumberGenerator ?? true;
        Description = $"{MinValue} ≤ n < {MaxValue}";
        isLoading = false;

        LoadHistorySessions();
    }

    private void SaveSettings()
    {
        if (isLoading || settings is null)
        {
            return;
        }
        settings.MinValue = MinValue;
        settings.MaxValue = MaxValue;
        settings.Range = Range;
        settings.IsUniqueEnabled = IsUniqueEnabled;
        settings.IsRangeEnabled = IsRangeEnabled;
        settings.IsStrongRandomNumberGenerator = IsStrongRandomNumberGenerator;
        settings.Save();
    }

    private void LoadHistorySessions()
    {
        HistorySessions.Clear();
        if (history is not null)
        {
            foreach (var session in history.Sessions.AsEnumerable().Reverse())
            {
                var sessionViewModel = new HistorySessionViewModel(session.StartTime);
                foreach (var entry in session.Entries)
                {
                    sessionViewModel.AddEntry(entry.Value, entry.Time);
                }
                HistorySessions.Add(sessionViewModel);
            }
        }
        UpdateHistorySummary();
    }

    private void RecordHistory(int value)
    {
        if (history is null)
        {
            return;
        }
        history.Append(value, DateTime.Now);
        if (currentSessionViewModel is null)
        {
            currentSessionViewModel = new HistorySessionViewModel(history.CurrentSession.StartTime);
            HistorySessions.Insert(0, currentSessionViewModel);
        }
        currentSessionViewModel.AddEntry(value, DateTime.Now);
        UpdateHistorySummary();
    }

    private void UpdateHistorySummary()
    {
        OnPropertyChanged(nameof(HistorySummary));
        OnPropertyChanged(nameof(HasHistory));
    }

    public async Task GenerateRandomNumber(CancellationToken token)
    {
        IsRunning = true;
        try
        {
            do
            {
                await timer.WaitForNextTickAsync(token);
                RandomNumber = randomNumberGenerator!.Next();
            }
            while (!token.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsRunning = false;
            cts?.Dispose();
            cts = null;
            if (GeneratedRandomNumbers.Count >= MaxValue - MinValue)
            {
                GeneratedRandomNumbers.Clear();
            }
            while (!IsUniqueNumber(RandomNumber) || !IsNumberInAllowedRange(RandomNumber))
            {
                RandomNumber = randomNumberGenerator!.Next();
            }
            GeneratedRandomNumbers.Add(RandomNumber);
            RecordHistory(RandomNumber);
        }
    }

    [RelayCommand]
    public void ClearGeneratedNumbers() => GeneratedRandomNumbers.Clear();

    [RelayCommand]
    private void ToggleStartStop()
    {
        if (IsRunning)
        {
            cts?.Cancel();
        }
        else
        {
            cts = new CancellationTokenSource();
            _ = GenerateRandomNumber(cts.Token);
        }
    }

    public bool IsUniqueNumber(int number) => !IsUniqueEnabled || !GeneratedRandomNumbers.Contains(number);

    public bool IsNumberInAllowedRange(int number)
    {
        if (!IsRangeEnabled)
        {
            return true;
        }
        var interval = (int)Math.Ceiling((MaxValue - MinValue) * 1.0 / Range);//计算划分成多少个区间
        var avgExistingNumbers = GeneratedRandomNumbers.Count * 1.0 / interval;//计算每个区间平均有几个数
        int rangeMinValue = MinValue + ((number - MinValue) / Range * Range);
        int rangeMaxValue = Math.Min(MaxValue, rangeMinValue + Range - 1);
        var res = GeneratedRandomNumbers.Count(t => t >= rangeMinValue && t <= rangeMaxValue) <= avgExistingNumbers;
        return res;
    }
}
