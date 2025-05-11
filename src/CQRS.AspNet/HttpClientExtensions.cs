namespace CQRS.AspNet;


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Mvc;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ProblemDetails? ProblemDetails { get; }
    public string? RawResponse { get; }

    public ApiException(HttpStatusCode statusCode, ProblemDetails? problemDetails, string? rawResponse, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ProblemDetails = problemDetails;
        RawResponse = rawResponse;
    }
}


public static class HttpClientExtensions
{

    public static async Task<HttpResponseMessage> SendAndHandleResponse(this HttpClient client, HttpRequestMessage httpRequest, Func<HttpResponseMessage, bool>? isSuccessful = null, CancellationToken cancellationToken = default)
    {
        isSuccessful ??= (responseMessage) => responseMessage.IsSuccessStatusCode;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        if (!isSuccessful(response))
        {
            await HandleErrorResponse(response);
        }
        return response;
    }

    public static async Task<TResult?> SendAndHandleResponse<TResult>(this HttpClient client, HttpRequestMessage httpRequest, Func<HttpResponseMessage, bool>? isSuccessful = null, CancellationToken cancellationToken = default)
    {
        var response = await client.SendAndHandleResponse(httpRequest, isSuccessful, cancellationToken);
        return await response.Content.As<TResult>();
    }

    private static async Task HandleErrorResponse(HttpResponseMessage response)
    {
        string? content = null;
        ProblemDetails? problemDetails = null;

        try
        {
            // Try to parse as ProblemDetails if content-type matches
            if (response.Content.Headers.ContentType?.MediaType == "application/problem+json" ||
                response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            }
        }
        catch (JsonException)
        {
            // If parsing fails, we'll just keep the raw content
        }
        if (problemDetails == null)
        {
            content = await response.Content.ReadAsStringAsync();
        }


        var message = problemDetails?.Title ?? content;
        throw new ApiException(
            response.StatusCode,
            problemDetails,
            content,
            $"HTTP request failed with status code {(int)response.StatusCode} ({response.StatusCode}): {message}"
        );
    }


    public static async Task<TResult> Get<TResult>(this HttpClient client, IQuery<TResult> query, Func<HttpResponseMessage, bool>? success = null, CancellationToken cancellationToken = default)
    {
        var route = query.GetType().GetCustomAttribute<GetAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholdersWithQueryParameters(route, query);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAndHandleResponse(httpRequest, success, cancellationToken);
        return await response.Content.As<TResult>();
    }

    public static async Task Post<TCommand>(this HttpClient client, TCommand command, Func<HttpResponseMessage, bool>? success = null, CancellationToken cancellationToken = default) where TCommand : class
    {
        // Get the uri from the PostAttribute found on TCommand
        var route = typeof(TCommand).GetCustomAttribute<PostAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholders(route, command);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequest.Content = JsonContent.Create(command);
        await client.SendAndHandleResponse(httpRequest, success, cancellationToken);
    }

}
