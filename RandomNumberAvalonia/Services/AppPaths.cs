using System;
using System.IO;

namespace RandomNumberAvalonia.Services;

/// <summary>
/// 应用数据文件路径（%APPDATA%\RandomNumberAvalonia）
/// </summary>
public static class AppPaths
{
    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RandomNumberAvalonia");

    public static string ConfigPath => Path.Combine(DataDirectory, "config.json");

    public static string HistoryPath => Path.Combine(DataDirectory, "history.json");
}
