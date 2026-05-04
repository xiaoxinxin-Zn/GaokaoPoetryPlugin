using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using GaokaoPoetryPlugin.Services;

namespace GaokaoPoetryPlugin.Components;

[ComponentInfo("12345678-1234-1234-1234-123456789012", "高考古诗文", "随机展示高考必背古诗文名句")]
public partial class PoetryComponent : ComponentBase<PoetryComponentSettings>
{
    private readonly PoetryService _poetryService = new();
    private System.Timers.Timer? _timer;

    public PoetryComponent()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _poetryService.LoadFromEmbeddedResource();

        if (!_poetryService.IsLoaded)
        {
            _ = _poetryService.LoadFromNetworkAsync().ContinueWith(_ =>
            {
                Dispatcher.UIThread.Invoke(() => RefreshPoetry());
            });
        }

        RefreshPoetry();
        StartTimer();

        if (Settings != null)
            Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        if (Settings != null)
            Settings.PropertyChanged -= OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PoetryComponentSettings.IntervalSeconds):
                Dispatcher.UIThread.Invoke(() =>
                {
                    _timer?.Stop();
                    _timer?.Dispose();
                    StartTimer();
                });
                break;
            case nameof(PoetryComponentSettings.BookRequiredUp):
            case nameof(PoetryComponentSettings.BookRequiredDown):
            case nameof(PoetryComponentSettings.BookSelectiveUp):
            case nameof(PoetryComponentSettings.BookSelectiveMiddle):
            case nameof(PoetryComponentSettings.BookSelectiveDown):
            case nameof(PoetryComponentSettings.BookOther):
                Dispatcher.UIThread.Invoke(() => RefreshPoetry());
                break;
            default:
                Dispatcher.UIThread.Invoke(() => RefreshPoetry());
                break;
        }
    }

    private void StartTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = new System.Timers.Timer((Settings?.IntervalSeconds ?? 30) * 1000);
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Invoke(() => RefreshPoetry());
        _timer.Start();
    }

    private void RefreshPoetry()
    {
        var allowed = Settings?.GetAllowedBooks() ?? new List<string>();
        var item = _poetryService.GetRandom(allowed);

        if (item == null || Settings == null)
        {
            PoetryTextBlock.Text = allowed.Count == 0 ? "请至少选择一个册别" : "正在加载...";
            return;
        }

        var sentence = item.content.Length > 0 ? item.content[0] : "";
        var parts = new List<string>();

        if (Settings.ShowAuthor && !string.IsNullOrEmpty(item.author))
            parts.Add(item.author);
        if (Settings.ShowTitle && !string.IsNullOrEmpty(item.title))
            parts.Add("《" + item.title + "》");
        parts.Add("[" + item.book + "]");

        var info = string.Join(" · ", parts);

        PoetryTextBlock.Text = string.IsNullOrEmpty(sentence)
            ? info
            : sentence + Environment.NewLine + info;
        PoetryTextBlock.FontSize = Settings.FontSize;
    }
}
