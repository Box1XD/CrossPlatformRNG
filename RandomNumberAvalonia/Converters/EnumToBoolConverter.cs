using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RandomNumberAvalonia.Converters;

/// <summary>
/// 枚举值与 ConverterParameter 相等比较。
/// 正向：枚举 → bool（页面 IsVisible）；
/// 反向：true + 参数 → 枚举（导航 RadioButton 的 IsChecked，双向绑定）。
/// </summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is not null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return BindingOperations.DoNothing;
    }
}
