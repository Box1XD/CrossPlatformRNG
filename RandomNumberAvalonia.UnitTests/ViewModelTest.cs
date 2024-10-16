using RandomNumberAvalonia.ViewModels;
using System.Diagnostics;

namespace RandomNumberAvalonia.UnitTests;

public class ViewModelTest
{
    MainViewModel viewModel = new();

    [Fact]
    public async Task TestUniqueEnabled()
    {
        viewModel.ClearGeneratedNumbers();
        int min = 0, max = 100;
        viewModel.MinValue = min;
        viewModel.MaxValue = max;
        viewModel.IsUniqueEnabled = true;
        viewModel.IsRangeEnabled = false;
        int count = max - min;

        for (int i = 0; i < count; i++)
        {
            using var cts = new CancellationTokenSource();
            var task = viewModel.GenerateRandomNumber(cts.Token);
            cts.Cancel();
            await task;
            Assert.True(viewModel.GeneratedRandomNumbers.Count == viewModel.GeneratedRandomNumbers.Distinct().Count());
        }
    }

    [Fact]
    public async Task TestUniqueDisabled()
    {
        viewModel.ClearGeneratedNumbers();
        int min = 0, max = 100;
        viewModel.MinValue = min;
        viewModel.MaxValue = max;
        viewModel.IsUniqueEnabled = false;
        viewModel.IsRangeEnabled = false;
        int count = max - min;

        for (int i = 0; i < count; i++)
        {
            using var cts = new CancellationTokenSource();
            var task = viewModel.GenerateRandomNumber(cts.Token);
            cts.Cancel();
            await task;
        }
        Assert.False(viewModel.GeneratedRandomNumbers.Count == viewModel.GeneratedRandomNumbers.Distinct().Count());
    }

    [Fact]
    public async Task TestRangeEnabled()
    {
        viewModel.ClearGeneratedNumbers();
        viewModel.IsUniqueEnabled = false;
        viewModel.IsRangeEnabled = true;
        for (int max = 100; max < 1000; max += 100)
        {
            for (int min = 0; min < max / 2; min += max / 10)
            {
                viewModel.MinValue = min;
                viewModel.MaxValue = max;
                int count = max - min;
                for (int range = 2; range < count / 10; range++)
                {
                    viewModel.Range = range;
                    int interval = (int)Math.Ceiling(1.0 * count / range);
                    for (int i = 0; i < count; i++)
                    {
                        using var cts = new CancellationTokenSource();
                        var task = viewModel.GenerateRandomNumber(cts.Token);
                        cts.Cancel();
                        await task;
                        for (int j = 0; j < interval; j++)
                        {
                            Assert.True(viewModel.GeneratedRandomNumbers.Count(r => r >= min + j * range && r < min + (j + 1) * range) <= Math.Ceiling(1.0 * viewModel.GeneratedRandomNumbers.Count / interval));
                        }
                    }
                    viewModel.GeneratedRandomNumbers.Clear();
                }
            }
        }
    }
}