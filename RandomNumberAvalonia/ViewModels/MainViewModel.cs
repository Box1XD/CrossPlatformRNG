using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using RandomNumberAvalonia.Core;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace RandomNumberAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PeriodicTimer timer;
    private IRandomNumberGenerator? randomNumberGenerator;

    [ObservableProperty]
    private HashSet<int> generatedRandomNumbers = [];

    [ObservableProperty]
    private int randomNumber = 0;

    [ObservableProperty]
    private int interval = 10;

    private int minValue;

    [Range(0, int.MaxValue - 1)]
    public int MinValue
    {
        get => minValue;
        set
        {
            SetProperty(ref minValue, value);
            if (randomNumberGenerator is not null)
            {
                randomNumberGenerator.MinValue = value;
            }
        }
    }

    private int maxValue;

    [Range(1, int.MaxValue)]
    public int MaxValue
    {
        get => maxValue;
        set
        {
            SetProperty(ref maxValue, value);
            if (randomNumberGenerator is not null)
            {
                randomNumberGenerator.MaxValue = value;
            }
        }
    }

    [ObservableProperty]
    private bool isUniqueEnabled = false;

    [ObservableProperty]
    private bool isIntervalEnabled = false;

    private bool isStrongRandomNumberGenerator;

    public bool IsStrongRandomNumberGenerator
    {
        get => isStrongRandomNumberGenerator;
        set
        {
            SetProperty(ref isStrongRandomNumberGenerator, value);
            randomNumberGenerator = isStrongRandomNumberGenerator ? StrongRandomNumberGenerator.Instance : FakeRandomNumberGenerator.Instance;
            randomNumberGenerator.MinValue = MinValue;
            randomNumberGenerator.MaxValue = MaxValue;
        }
    }

    public MainViewModel()
    {
        timer = new(TimeSpan.FromMilliseconds(1));
        MinValue = 0;
        MaxValue = 100;
        IsStrongRandomNumberGenerator = true;
        IsIntervalEnabled = true;
        IsUniqueEnabled = true;
    }

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
            while (!IsUniqueNumber(RandomNumber) || !IsNumberInAllowedRange(RandomNumber))
            {
                RandomNumber = await randomNumberGenerator!.Next();
            }
        }
        finally
        {
            GeneratedRandomNumbers.Add(RandomNumber);
        }
    }

    [RelayCommand]
    public void ClearGeneratedNumbers() => GeneratedRandomNumbers.Clear();

    public bool IsUniqueNumber(int number) => !IsUniqueEnabled || !GeneratedRandomNumbers.Contains(number);

    public bool IsNumberInAllowedRange(int number)
    {
        if (!IsIntervalEnabled)
        {
            return true;
        }
        var ranges = (int)Math.Ceiling((MaxValue - MinValue) * 1.0 / Interval);//计算划分成多少个区间
        var avgExistingNumbers = GeneratedRandomNumbers.Count * 1.0 / ranges;//计算每个区间平均有几个数
        int rangeMinValue = (number / ranges) * ranges;
        int rangeMaxValue = rangeMinValue + ranges - 1;
        return GeneratedRandomNumbers.Count(t => t >= rangeMinValue && t <= rangeMaxValue) <= avgExistingNumbers;
    }
}