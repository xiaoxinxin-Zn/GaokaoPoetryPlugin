using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GaokaoPoetryPlugin.Models;

namespace GaokaoPoetryPlugin.Services;

public class PoetryService
{
    private readonly List<PoetryItem> _items = new();
    private readonly Random _random = new();
    private readonly HttpClient _http = new();
    private bool _loaded = false;

    public bool IsLoaded => _loaded;
    public int Count => _items.Count;

    public void LoadFromEmbeddedResource()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("divided.json"));

            if (resourceName == null) return;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var items = JsonSerializer.Deserialize<List<PoetryItem>>(json);
            if (items != null)
            {
                _items.Clear();
                _items.AddRange(items);
                _loaded = true;
            }
        }
        catch { /* 忽略嵌入资源加载错误 */ }
    }

    public async Task LoadFromNetworkAsync()
    {
        if (_loaded) return;
        try
        {
            const string url = "https://clover-yan.github.io/gaokao-poetry/generated/divided.json";
            var json = await _http.GetStringAsync(url);
            var items = JsonSerializer.Deserialize<List<PoetryItem>>(json);
            if (items != null)
            {
                _items.Clear();
                _items.AddRange(items);
                _loaded = true;
            }
        }
        catch { /* 网络加载失败 */ }
    }

    public PoetryItem? GetRandom(List<string> allowedBooks)
    {
        var filtered = _items.Where(i => allowedBooks.Contains(i.book)).ToList();
        if (filtered.Count == 0) return null;
        return filtered[_random.Next(filtered.Count)];
    }

    public List<string> GetAllBooks()
    {
        return _items.Select(i => i.book).Distinct().OrderBy(b => b).ToList();
    }
}
