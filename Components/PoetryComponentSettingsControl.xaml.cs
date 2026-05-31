using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;

namespace GaokaoPoetryPlugin.Components;

public partial class PoetryComponentSettingsControl : ComponentBase<PoetryComponentSettings>
{
    public PoetryComponentSettingsControl()
    {
        InitializeComponent();
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        DataContext = this;
    }
}
