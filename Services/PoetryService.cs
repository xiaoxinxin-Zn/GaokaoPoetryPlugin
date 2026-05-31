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

/// <summary>
/// 古诗文数据服务：负责加载数据（嵌入资源 / 网络回退）和随机选取。
/// </summary>
public class PoetryService
{
    private readonly List<PoetryItem> _items = new();
    private readonly Random _random = new();
    private readonly HttpClient _http = new();
    private bool _loaded = false;

    /// <summary>是否已成功加载数据</summary>
    public bool IsLoaded => _loaded;

    /// <summary>已加载的条目数量</summary>
    public int Count => _items.Count;

    /// <summary>最近一次加载的错误信息（null 表示无错误）</summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// 从嵌入资源加载 divided.json（离线模式）。
    /// </summary>
    /// <returns>是否加载成功</returns>
    public bool LoadFromEmbeddedResource()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("divided.json"));

            if (resourceName == null)
            {
                LastError = "嵌入资源中未找到 divided.json";
                return false;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                LastError = "无法读取嵌入资源流";
                return false;
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var items = JsonSerializer.Deserialize<List<PoetryItem>>(json);
            if (items != null && items.Count > 0)
            {
                _items.Clear();
                _items.AddRange(items);
                _loaded = true;
                LastError = null;
                return true;
            }

            LastError = "嵌入资源数据为空或格式错误";
            return false;
        }
        catch (Exception ex)
        {
            LastError = $"嵌入资源加载失败: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 从网络加载数据（在线回退模式）。
    /// </summary>
    /// <returns>是否加载成功</returns>
    public async Task<bool> LoadFromNetworkAsync()
    {
        if (_loaded) return true;

        try
        {
            const string url = "https://clover-yan.github.io/gaokao-poetry/generated/divided.json";
            var json = await _http.GetStringAsync(url);
            var items = JsonSerializer.Deserialize<List<PoetryItem>>(json);
            if (items != null && items.Count > 0)
            {
                _items.Clear();
                _items.AddRange(items);
                _loaded = true;
                LastError = null;
                return true;
            }

            LastError = "网络数据为空或格式错误";
            return false;
        }
        catch (HttpRequestException ex)
        {
            LastError = $"网络请求失败: {ex.Message}";
            return false;
        }
        catch (TaskCanceledException)
        {
            LastError = "网络请求超时";
            return false;
        }
        catch (Exception ex)
        {
            LastError = $"网络加载失败: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 从允许的册别列表中随机选取一条。
    /// </summary>
    public PoetryItem? GetRandom(List<string> allowedBooks)
    {
        var filtered = _items.Where(i => allowedBooks.Contains(i.book)).ToList();
        if (filtered.Count == 0) return null;
        return filtered[_random.Next(filtered.Count)];
    }

    /// <summary>
    /// 获取数据中所有不重复的册别。
    /// </summary>
    public List<string> GetAllBooks()
    {
        return _items.Select(i => i.book).Distinct().OrderBy(b => b).ToList();
    }
}
