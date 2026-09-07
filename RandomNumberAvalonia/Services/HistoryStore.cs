using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RandomNumberAvalonia.Services;

/// <summary>单条历史记录：生成的数字和时间。</summary>
public sealed class HistoryEntry
{
    public int Value { get; set; }

    public DateTime Time { get; set; }
}

/// <summary>一次启动（会话）产生的所有记录。</summary>
public sealed class HistorySession
{
    public DateTime StartTime { get; set; }

    public List<HistoryEntry> Entries { get; set; } = [];
}

/// <summary>
/// 历史记录存储：启动时读取 history.json，
/// 每次启动视为一次新会话，每生成一个数字立即追加并保存。
/// </summary>
public sealed class HistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string path;

    /// <summary>历史文件中的所有会话（旧在前，新在后）。</summary>
    public List<HistorySession> Sessions { get; } = [];

    /// <summary>本次启动对应的会话；生成第一个数字后才真正写入文件。</summary>
    public HistorySession CurrentSession { get; } = new() { StartTime = DateTime.Now };

    public HistoryStore(string path)
    {
        this.path = path;
        try
        {
            if (File.Exists(path))
            {
                var sessions = JsonSerializer.Deserialize<List<HistorySession>>(File.ReadAllText(path), JsonOptions);
                if (sessions is not null)
                {
                    Sessions.AddRange(sessions);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // 历史文件损坏时从空记录开始
        }
    }

    /// <summary>记录一次生成的数字。当前会话首次产生记录时才计入会话列表。</summary>
    public void Append(int value, DateTime time)
    {
        if (CurrentSession.Entries.Count == 0)
        {
            Sessions.Add(CurrentSession);
        }
        CurrentSession.Entries.Add(new HistoryEntry { Value = value, Time = time });
        Save();
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(Sessions, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 保存失败不影响使用
        }
    }
}
