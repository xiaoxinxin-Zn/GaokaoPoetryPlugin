using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GaokaoPoetryPlugin.Components;

public class PoetryComponentSettings : INotifyPropertyChanged
{
    private int _intervalSeconds = 30;
    private bool _showAuthor = true;
    private bool _showTitle = true;
    private double _fontSize = 14;
    private bool _bookRequiredUp = true;
    private bool _bookRequiredDown = true;
    private bool _bookSelectiveUp = true;
    private bool _bookSelectiveMiddle = true;
    private bool _bookSelectiveDown = true;
    private bool _bookOther = true;

    public int IntervalSeconds
    {
        get => _intervalSeconds;
        set { _intervalSeconds = value; OnPropertyChanged(); }
    }

    public bool ShowAuthor
    {
        get => _showAuthor;
        set { _showAuthor = value; OnPropertyChanged(); }
    }

    public bool ShowTitle
    {
        get => _showTitle;
        set { _showTitle = value; OnPropertyChanged(); }
    }

    public double FontSize
    {
        get => _fontSize;
        set { _fontSize = value; OnPropertyChanged(); }
    }

    public bool BookRequiredUp
    {
        get => _bookRequiredUp;
        set { _bookRequiredUp = value; OnPropertyChanged(); }
    }

    public bool BookRequiredDown
    {
        get => _bookRequiredDown;
        set { _bookRequiredDown = value; OnPropertyChanged(); }
    }

    public bool BookSelectiveUp
    {
        get => _bookSelectiveUp;
        set { _bookSelectiveUp = value; OnPropertyChanged(); }
    }

    public bool BookSelectiveMiddle
    {
        get => _bookSelectiveMiddle;
        set { _bookSelectiveMiddle = value; OnPropertyChanged(); }
    }

    public bool BookSelectiveDown
    {
        get => _bookSelectiveDown;
        set { _bookSelectiveDown = value; OnPropertyChanged(); }
    }

    public bool BookOther
    {
        get => _bookOther;
        set { _bookOther = value; OnPropertyChanged(); }
    }

    public List<string> GetAllowedBooks()
    {
        var list = new List<string>();
        if (BookRequiredUp) list.Add("必修上");
        if (BookRequiredDown) list.Add("必修下");
        if (BookSelectiveUp) list.Add("选择性必修上");
        if (BookSelectiveMiddle) list.Add("选择性必修中");
        if (BookSelectiveDown) list.Add("选择性必修下");
        if (BookOther) list.Add("其他");
        return list;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
