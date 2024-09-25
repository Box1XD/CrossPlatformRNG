using System.Security.Cryptography;

namespace RandomNumberAvalonia.Core;

public sealed class FakeRandomNumberGenerator : IRandomNumberGenerator
{
    private Random randomNumberGenerator;

    private readonly SemaphoreSlim semaphore;

    private int fromInclusive = 0;

    private int toExclusive = 1;

    private static readonly Lazy<FakeRandomNumberGenerator> lazyInstance = new(() => new());

    public static FakeRandomNumberGenerator Instance => lazyInstance.Value;

    public int MinValue
    {
        get => fromInclusive;
        set
        {
            fromInclusive = value >= 0 ? value : 0;
            randomNumberGenerator = new Random(RandomNumberGenerator.GetInt32(39393939));
        }
    }

    public int MaxValue
    {
        get => toExclusive;
        set
        {
            toExclusive = value > MinValue ? value : MinValue + 1;

            randomNumberGenerator = new Random(RandomNumberGenerator.GetInt32(39393939));
        }
    }

    private FakeRandomNumberGenerator()
    {
        randomNumberGenerator = new Random(RandomNumberGenerator.GetInt32(39393939));
        semaphore = new(1, 1);
    }

    public async Task<int> Next()
    {
        await semaphore.WaitAsync();
        int result;
        result = randomNumberGenerator.Next(MinValue, MaxValue);
        semaphore.Release();
        return result;
    }
}