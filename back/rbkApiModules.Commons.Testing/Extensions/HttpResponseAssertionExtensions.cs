using System.Net;
using rbkApiModules.Commons.Testing;
using Shouldly;

namespace rbkApiModules.Testting.Core;

public static class HttpAssertionExtensions
{
    public static void ShouldHaveErrors(this HttpResponse response, HttpStatusCode expectedCode, params string[] messages)
    {
        response.Code.ShouldBe(expectedCode, $"Unexpected http response code. Should be {expectedCode} but was {response.Code}");

        response.Messages.Length.ShouldBe(messages.Length, $"Unexpected number of error messages. Should be {messages.Length} but was {response.Messages.Length}");

        foreach (var message in messages)
        {
            response.Messages.ShouldContain(message, $"Could not find the [{message}] message in the response");
        }
    }

    public static void ShouldHaveErrors<T>(this HttpResponse<T> response, HttpStatusCode expectedCode, params string[] messages) where T : class
    {
        response.Code.ShouldBe(expectedCode, $"Unexpected http response code. Should be {expectedCode} but was {response.Code}");

        response.Data.ShouldBeNull($"Expected response of type {typeof(T).Name}, but the response was not empty");
        response.Messages.Length.ShouldBe(messages.Length, $"Unexpected number of error messages. Should be {messages.Length} but was {response.Messages.Length}");

        foreach (var message in messages)
        {
            response.Messages.ShouldContain(message, $"Could not find the [{message}] message in the response");
        }
    }

    public static void ShouldRedirect(this HttpResponse response, string url)
    {
        response.Code.ShouldBe(HttpStatusCode.Redirect);

        response.Messages.Length.ShouldBe(1, $"Unexpected number of messages. Should be 1 but was {response.Messages.Length}");

        response.Messages[0].ShouldBe(url, $"Unexpected redirect url. Should be {url} but was {response.Messages[0]}");
    }

    public static void ShouldBeSuccess<T>(this HttpResponse<T> response, out T result) where T : class
    {
        response.IsSuccess.ShouldBeTrue($"Expected success response, but the response was not successful. Messages: [ {string.Join(", ", response.Messages)} ]");
        response.Data.ShouldNotBeNull($"Expected response of type {typeof(T).Name}, but the response was empty");
        response.Data.ShouldBeOfType(typeof(T), $"Expected response of type {typeof(T).Name}, but the response was of type {response.Data.GetType().Name}");

        result = response.Data;
    }

    public static void ShouldBeSuccess(this HttpResponse response)
    {
        string messages;
        if (response.Messages.Any())
        {
            messages = string.Join(", ", response.Messages);
        }
        else
        {
            switch (response.Code)
            {
                case HttpStatusCode.BadRequest:
                    messages = "Response might have validation errors. Please check the contents of the response.";
                    break;
                case HttpStatusCode.Unauthorized:
                    messages = "Request was not authenticated. Did you pass the correct credentials?";
                    break;
                case HttpStatusCode.Forbidden:
                    messages = "User was not authorized to do this action, Did you pass the correct credentials?";
                    break;
                case HttpStatusCode.NotFound:
                    messages = "Endpoint was not found. Is the Url correct and is the endpoint registered?";
                    break;
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.UnsupportedMediaType:
                    messages = "Http method might be wrong. Please check if the endpoint mapping is correct or if the test is calling the correct http method.";
                    break;
                case HttpStatusCode.InternalServerError:
                    messages = "Request was interrupted because of a critical failure. Please check the contents of the response. " + response.Body;
                    break;
                default:
                    messages = $"Error code: {response.Code}";
                    break;
            }
        }

        response.IsSuccess.ShouldBeTrue($"Http request failed. Messages: [ {messages} ]");
    }

    public static void ShouldBeForbidden(this HttpResponse response)
    {
        response.Code.ShouldBe(HttpStatusCode.Forbidden, $"Resquest bypassed authorization. Messages: [ {string.Join(", ", response.Messages)} ]");
    }
}

