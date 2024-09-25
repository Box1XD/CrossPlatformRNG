using RandomNumberAvalonia.Core;
using System.Diagnostics;

namespace RandomNumberAvalonia.Core.UnitTests;

public class Test_UniqueRandomNumberGenerator
{
    [Fact]
    public async Task TestStrongRandomNumberGeneratorSpeed()
    {
        var generator = StrongRandomNumberGenerator.Instance;
        generator.MinValue = 0;
        generator.MaxValue = 100;
        var sw = Stopwatch.StartNew();
        int number;
        int count = 0;
        while (sw.ElapsedMilliseconds < 1000)
        {
            number = await generator.Next();
            count++;
        }
        sw.Stop();
        Debug.WriteLine($"StrongRandomNumberGenerator Generated {count} numbers in {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task TestFakeRandomNumberGeneratorSpeed()
    {
        var generator = FakeRandomNumberGenerator.Instance;
        generator.MinValue = 0;
        generator.MaxValue = 100;
        var sw = Stopwatch.StartNew();
        int number;
        int count = 0;
        while (sw.ElapsedMilliseconds < 1000)
        {
            number = await generator.Next();
            count++;
        }
        sw.Stop();
        Debug.WriteLine($"FakeRandomNumberGenerator Generated {count} numbers in {sw.ElapsedMilliseconds} ms");
    }
}