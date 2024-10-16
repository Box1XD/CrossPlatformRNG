using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RandomNumberAvalonia.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RandomNumberAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PeriodicTimer timer;
    private IRandomNumberGenerator? randomNumberGenerator;

    [ObservableProperty]
    private bool isPanelOpen = false;

    [ObservableProperty]
    private ObservableCollection<int> generatedRandomNumbers = [];

    [ObservableProperty]
    private int randomNumber = 0;

    [ObservableProperty]
    private int range = 10;

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
        Description = $"{MinValue}≤Number<{MaxValue}";
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
        Description = $"{MinValue}≤Number<{MaxValue}";
    }

    [ObservableProperty]
    private bool isUniqueEnabled = false;

    [ObservableProperty]
    private bool isRangeEnabled = false;

    [ObservableProperty]
    private bool isStrongRandomNumberGenerator;

    partial void OnIsStrongRandomNumberGeneratorChanged(bool value)
    {
        randomNumberGenerator = value ? StrongRandomNumberGenerator.Instance : FakeRandomNumberGenerator.Instance;
        randomNumberGenerator.MinValue = value ? MinValue - 1 : MinValue;
        randomNumberGenerator.MaxValue = MaxValue;
    }

    [ObservableProperty]
    string description = string.Empty;

    public MainViewModel()
    {
        timer = new(TimeSpan.FromMilliseconds(1));
        IsStrongRandomNumberGenerator = true;
        MinValue = 0;
        MaxValue = 100;
        IsRangeEnabled = true;
        IsUniqueEnabled = true;
    }

    [RelayCommand]
    private void TriggerPanel() => IsPanelOpen = !IsPanelOpen;

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task GenerateRandomNumber(CancellationToken token)
    {
        try
        {
            do
            {
                await timer.WaitForNextTickAsync(token);
                RandomNumber = await randomNumberGenerator!.Next();
            }
            while (!token.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (GeneratedRandomNumbers.Count >= MaxValue - MinValue)
            {
                GeneratedRandomNumbers.Clear();
            }
            while (!IsUniqueNumber(RandomNumber) || !IsNumberInAllowedRange(RandomNumber))
            {
                RandomNumber = await randomNumberGenerator!.Next();
            }
            GeneratedRandomNumbers.Add(RandomNumber);
        }
    }

    [RelayCommand]
    public void ClearGeneratedNumbers() => GeneratedRandomNumbers.Clear();

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