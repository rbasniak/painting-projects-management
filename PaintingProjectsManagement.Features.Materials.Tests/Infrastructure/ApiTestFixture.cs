using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; 
using System.Net.Http.Json;

namespace PaintingProjectsManagement.Features.Materials.Tests.Infrastructure;

public class ApiTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
        });
    }
    
    public async Task<T> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
    
    public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        var client = CreateClient();
        return await client.PostAsJsonAsync(url, data);
    }
    
    public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        var client = CreateClient();
        return await client.PutAsJsonAsync(url, data);
    }
    
    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var client = CreateClient();
        return await client.DeleteAsync(url);
    }
}