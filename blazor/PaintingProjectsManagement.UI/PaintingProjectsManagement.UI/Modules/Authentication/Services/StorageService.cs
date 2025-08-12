using Microsoft.JSInterop;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public interface IStorageService
{
    Task SetItemAsync(string key, string value, StorageType storageType = StorageType.Session);
    Task<string?> GetItemAsync(string key, StorageType storageType = StorageType.Session);
    Task RemoveItemAsync(string key, StorageType storageType = StorageType.Session);
    Task ClearAsync(StorageType storageType = StorageType.Session);
}

public enum StorageType
{
    Session,
    Local
}

public class StorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, object> _inMemoryCache = new();

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value, StorageType storageType = StorageType.Session)
    {
        // Always cache in memory for immediate access
        _inMemoryCache[key] = value;

        try
        {
            var storageName = storageType == StorageType.Session ? "sessionStorage" : "localStorage";
            await _jsRuntime.InvokeVoidAsync($"{storageName}.setItem", key, value);
        }
        catch (JSException ex)
        {
            // Log the error but don't throw - in-memory cache still works
            Console.WriteLine($"Storage write failed for key '{key}': {ex.Message}");
        }
    }

    public async Task<string?> GetItemAsync(string key, StorageType storageType = StorageType.Session)
    {
        // Try in-memory cache first
        if (_inMemoryCache.ContainsKey(key))
            return _inMemoryCache[key]?.ToString();

        try
        {
            var storageName = storageType == StorageType.Session ? "sessionStorage" : "localStorage";
            var value = await _jsRuntime.InvokeAsync<string>($"{storageName}.getItem", key);
            
            if (value != null)
                _inMemoryCache[key] = value;
                
            return value;
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Storage read failed for key '{key}': {ex.Message}");
            return null;
        }
    }

    public async Task RemoveItemAsync(string key, StorageType storageType = StorageType.Session)
    {
        _inMemoryCache.Remove(key);

        try
        {
            var storageName = storageType == StorageType.Session ? "sessionStorage" : "localStorage";
            await _jsRuntime.InvokeVoidAsync($"{storageName}.removeItem", key);
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Storage remove failed for key '{key}': {ex.Message}");
        }
    }

    public async Task ClearAsync(StorageType storageType = StorageType.Session)
    {
        _inMemoryCache.Clear();

        try
        {
            var storageName = storageType == StorageType.Session ? "sessionStorage" : "localStorage";
            await _jsRuntime.InvokeVoidAsync($"{storageName}.clear");
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Storage clear failed: {ex.Message}");
        }
    }
} 