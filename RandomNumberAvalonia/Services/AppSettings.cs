using System;
using System.IO;
using System.Text.Json;

namespace RandomNumberAvalonia.Services;

/// <summary>
/// 应用配置：启动时自动读取，任何更改后调用 <see cref="Save"/> 写回 config.json。
/// </summary>
public sealed class AppSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private string path = string.Empty;

    public int MinValue { get; set; } = 0;

    public int MaxValue { get; set; } = 100;

    public bool IsUniqueEnabled { get; set; } = true;

    public bool IsRangeEnabled { get; set; } = true;

    public int Range { get; set; } = 10;

    public bool IsStrongRandomNumberGenerator { get; set; } = true;

    /// <summary>
    /// 读取配置文件；文件不存在或损坏时返回默认配置。
    /// </summary>
    public static AppSettings LoadOrDefault(string path)
    {
        AppSettings? loaded = null;
        try
        {
            if (File.Exists(path))
            {
                loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOptions);
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // 配置损坏时回退到默认值
        }

        loaded ??= new AppSettings();
        loaded.path = path;

        // 数据合法性兜底
        if (loaded.MinValue < 0)
        {
            loaded.MinValue = 0;
        }
        if (loaded.MaxValue <= loaded.MinValue)
        {
            loaded.MinValue = 0;
            loaded.MaxValue = 100;
        }
        if (loaded.Range < 1)
        {
            loaded.Range = 10;
        }
        return loaded;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 保存失败不影响使用
        }
    }
}
