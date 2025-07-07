using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace rbkApiModules.Commons.Core.Testing;

public static class HttpAssertionExtensions
{
    public static void ShouldHaveErrors(this HttpResponse response, HttpStatusCode expectedCode, params string[] messages)
    {
        Assert.AreEqual(expectedCode, response.Code, "Unexpected http response code");

        Assert.AreEqual(messages.Length, response.Messages.Length, "Unexpected number of error messages");

        foreach (var message in messages)
        {
            Assert.Contains(message, response.Messages);
        }
    }

    public static void ShouldHaveErrors<T>(this HttpResponse<T> response, HttpStatusCode expectedCode, params string[] messages) where T : class
    {
        Assert.AreEqual(expectedCode, response.Code, "Unexpected http response code");

        Assert.IsNull(response.Data, "Expected response data to be null, but it was not");

        Assert.AreEqual(messages.Length, response.Messages.Length, "Unexpected number of error messages");

        foreach (var message in messages)
        {
            Assert.Contains(message, response.Messages);
        }
    }

    public static void ShouldRedirect(this HttpResponse response, string url)
    {
        Assert.AreEqual(HttpStatusCode.Redirect, response.Code, "Unexpected http response code");

        Assert.AreEqual(1, response.Messages.Length);

        Assert.AreEqual(url, response.Messages[0]);
    }

    public static void ShouldBeSuccess<T>(this HttpResponse<T> response, out T result) where T : class
    {
        Assert.IsTrue(response.IsSuccess, "Expected response to be successful, but it was not");
        Assert.IsNotNull(response.Data, $"Expected response of type {typeof(T).Name}, but the response was empty");

        result = response.Data;
    }

    public static void ShouldBeSuccess(this HttpResponse response)
    {
        Assert.IsTrue(response.IsSuccess, "Expected response to be successful, but it was not");
    }

    public static void ShouldBeForbidden(this HttpResponse response)
    {
        Assert.AreEqual(HttpStatusCode.Forbidden, response.Code, "Expected response code to be Forbidden");
    }
}