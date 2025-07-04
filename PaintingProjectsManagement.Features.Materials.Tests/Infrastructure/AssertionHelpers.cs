using System.Net;
using System.Text.Json;

namespace PaintingProjectsManagement.Features.Materials.Tests.Infrastructure;

public static class AssertionHelpers
{
    public static async Task<T> ShouldBeSuccessful<T>(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.ShouldBeTrue($"Expected success status code but got {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    
    public static async Task<string> ShouldBeBadRequest(this HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var errorObject = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        return errorObject?.Error;
    }
    
    private class ErrorResponse
    {
        public string Error { get; set; }
    }
}