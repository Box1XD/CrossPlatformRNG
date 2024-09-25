using System.Security.Cryptography;

namespace RandomNumberAvalonia.Core;

/// <summary>
/// 线程安全的强随机数生成器
/// </summary>
public sealed class StrongRandomNumberGenerator : IRandomNumberGenerator
{
    //private bool isIntervalEnabled;

    //private bool isUniqueEnabled;

    //private HashSet<uint> generatedNumbers;

    //private ConcurrentDictionary<int, int>? IntervalDictionary;

    private readonly SemaphoreSlim semaphore;

    private int fromInclusive = 0;

    private int toExclusive = 1;
    //private int interval = 1;

    private static readonly Lazy<StrongRandomNumberGenerator> lazyInstance = new(() => new());

    public static StrongRandomNumberGenerator Instance => lazyInstance.Value;

    public int MinValue
    {
        get => fromInclusive;
        set => fromInclusive = value >= 0 ? value : 0;
    }

    public int MaxValue
    {
        get => toExclusive;
        set
        {
            toExclusive = value > MinValue ? value : MinValue + 1;
        }
    }

    //public int Interval
    //{
    //    get => interval;
    //    set => interval = value > 1 ? value : 1;
    //}

    private StrongRandomNumberGenerator()
    {
        // generatedNumbers = [];
        semaphore = new(1, 1);
    }

    public async Task<int> Next()
    {
        await semaphore.WaitAsync();
        int result;
        result = RandomNumberGenerator.GetInt32(MinValue, MaxValue);

        semaphore.Release();
        return result;
    }

    //public void ChangeIntervalStatus(bool isIntervalEnabled)
    //{
    //    this.isIntervalEnabled = isIntervalEnabled;
    //    if (isIntervalEnabled)
    //    {
    //    }
    //    else
    //    {
    //    }
    //}

    //public void ChangeUniqueStatus(bool isUniqueEnabled)
    //{
    //    this.isUniqueEnabled = isUniqueEnabled;
    //    if (isUniqueEnabled)
    //    {
    //    }
    //    else
    //    {
    //    }
    //}
}