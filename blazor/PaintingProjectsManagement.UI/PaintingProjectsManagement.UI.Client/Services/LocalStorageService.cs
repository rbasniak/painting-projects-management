using Microsoft.JSInterop;
using System.Text.Json;

namespace PaintingProjectsManagement.UI.Client.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    private readonly Dictionary<string, object> _localStorageCache = new();

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            if (!_localStorageCache.TryGetValue(key, out var value))
            {
                //var valueStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                //if (valueStr != null)
                //{
                //    value = JsonSerializer.Deserialize<T>(valueStr);
                //    _localStorageCache[key] = value!;
                //}
                //else
                {
                    return default;
                }
            }

            return (T)value!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving item with key '{key}' from local storage.", ex);
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            if (_localStorageCache.ContainsKey(key))
            {
                _localStorageCache[key] = value!;
            }
            else
            {
                _localStorageCache.Add(key, value!);
            }

            //await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error setting item with key '{key}' in local storage.", ex);
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            if (_localStorageCache.ContainsKey(key))
            {
                _localStorageCache.Remove(key);
            }

            //await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error removing item with key '{key}' from local storage.", ex);
        }
    }
}