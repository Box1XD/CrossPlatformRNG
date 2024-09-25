namespace RandomNumberAvalonia.Core;

public interface IRandomNumberGenerator
{
    int MinValue { get; set; }
    int MaxValue { get; set; }

    Task<int> Next();
}