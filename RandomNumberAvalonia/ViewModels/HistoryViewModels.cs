using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RandomNumberAvalonia.ViewModels;

/// <summary>历史记录页中一次启动（会话）的展示模型。</summary>
public partial class HistorySessionViewModel : ViewModelBase
{
    /// <summary>本次启动的时间（完整显示到秒）。</summary>
    public string Header { get; }

    /// <summary>本次启动生成的数字，新的在前。</summary>
    public ObservableCollection<HistoryEntryViewModel> Entries { get; } = [];

    public string CountText => $"共 {Entries.Count} 个";

    public HistorySessionViewModel(DateTime startTime)
    {
        Header = startTime.ToString("yyyy-MM-dd HH:mm:ss");
        Entries.CollectionChanged += OnEntriesChanged;
    }

    /// <summary>新增一条记录（新的排在最前面）。</summary>
    public void AddEntry(int value, DateTime time) => Entries.Insert(0, new HistoryEntryViewModel(value, time));

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(CountText));
}

/// <summary>历史记录页中单个数字的展示模型。</summary>
public sealed class HistoryEntryViewModel
{
    public string ValueText { get; }

    public string TimeText { get; }

    public HistoryEntryViewModel(int value, DateTime time)
    {
        ValueText = value.ToString();
        TimeText = time.ToString("HH:mm:ss");
    }
}
