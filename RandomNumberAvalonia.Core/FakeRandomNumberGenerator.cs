namespace RandomNumberAvalonia.Core;

public sealed class FakeRandomNumberGenerator : IRandomNumberGenerator
{
    private int fromInclusive = 0;

    private int toExclusive = 1;

    private static readonly Lazy<FakeRandomNumberGenerator> lazyInstance = new(() => new());

    public static FakeRandomNumberGenerator Instance => lazyInstance.Value;

    public int MinValue
    {
        get => fromInclusive;
        set => fromInclusive = value >= 0 ? value : 0;
    }

    public int MaxValue
    {
        get => toExclusive;
        set => toExclusive = value > MinValue ? value : MinValue + 1;
    }

    private FakeRandomNumberGenerator()
    {
    }

    public int Next()
    {
        // Random.Shared is thread-safe
        return Random.Shared.Next(MinValue, MaxValue);
    }
}