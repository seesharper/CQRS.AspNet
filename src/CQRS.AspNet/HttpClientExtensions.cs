namespace CQRS.AspNet;


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Mvc;



public static class HttpClientExtensions
{

    public static async Task<HttpResponseMessage> SendAndHandleResponse(this HttpClient client, HttpRequestMessage httpRequest, Func<HttpResponseMessage, bool>? isSuccessful = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)
    {
        isSuccessful ??= (responseMessage) => responseMessage.IsSuccessStatusCode;
        errorHandler ??= HandleErrorResponse;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        if (!isSuccessful(response))
        {
            await errorHandler(response);
        }
        return response;
    }

    public static async Task<TResult?> SendAndHandleResponse<TResult>(this HttpClient client, HttpRequestMessage httpRequest, Func<HttpResponseMessage, bool>? isSuccessful = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)
    {
        var response = await client.SendAndHandleResponse(httpRequest, isSuccessful, errorHandler, cancellationToken);
        return await response.Content.As<TResult>(cancellationToken: cancellationToken);
    }

    private static async Task HandleErrorResponse(HttpResponseMessage response)
    {
        if (response.HasProblemDetails())
        {
            var problemDetails = await response.Content.As<ProblemDetails>();
            var problemDetailsMessage = CreateErrorMessageFromProblemDetails(problemDetails);
            var message = $"HTTP request ({GetUrlFromResponseMessage(response)}) failed with status code {(int)response.StatusCode} ({response.StatusCode}): {response.ReasonPhrase}. Problem Details: {problemDetailsMessage}";
            throw new HttpRequestException(message, null, response.StatusCode);
        }
        else
        {
            var stringResponse = await response.Content.ReadAsStringAsync();

            // If the response does not have ProblemDetails, we can throw a generic exception with the status code and reason phrase.
            var message = $"HTTP request ({GetUrlFromResponseMessage(response)}) failed with status code {(int)response.StatusCode} ({response.StatusCode}): {response.ReasonPhrase} The raw string response was: {stringResponse}";
            throw new HttpRequestException(message, null, response.StatusCode);
        }
    }

    private static string CreateErrorMessageFromProblemDetails(ProblemDetails problemDetails)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title: {problemDetails.Title}");
        sb.AppendLine($"Status: {problemDetails.Status}");
        sb.AppendLine($"Detail: {problemDetails.Detail}");
        sb.AppendLine($"Instance: {problemDetails.Instance}");
        if (problemDetails.Extensions != null)
        {
            foreach (var extension in problemDetails.Extensions)
            {
                sb.AppendLine($"{extension.Key}: {extension.Value}");
            }
        }
        return sb.ToString();
    }

    private static string GetUrlFromResponseMessage(HttpResponseMessage response)
    {
        if (response.RequestMessage == null)
        {
            return string.Empty;
        }
        return response.RequestMessage.RequestUri?.ToString() ?? string.Empty;
    }




    public static async Task<TResult> Get<TResult>(this HttpClient client, IQuery<TResult> query, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)
    {
        var route = query.GetType().GetCustomAttribute<GetAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholdersWithQueryParameters(route, query);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAndHandleResponse(httpRequest, success, errorHandler, cancellationToken);
        return await response.Content.As<TResult>(cancellationToken: cancellationToken);
    }


    // Todo create an overload that provides the route 
    public static async Task<TResult> Get<TResult>(this HttpClient client, GetQuery<TResult> query, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)
    {
        var route = query.GetType().GetCustomAttribute<GetAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholdersWithQueryParameters(route, query);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAndHandleResponse(httpRequest, success, errorHandler, cancellationToken);
        return await response.Content.As<TResult>(cancellationToken: cancellationToken);
    }


    public static async Task<TResult> Post<TResult>(this HttpClient client, PostCommand<TResult> command, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)

    {
        var response = await PostAndHandleResponse(client, command, success, errorHandler, cancellationToken);
        return await response.Content.As<TResult>(cancellationToken: cancellationToken);
    }

    public static async Task<HttpResponseMessage> Post(this HttpClient client, PostCommand command, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default)
    {
        return await PostAndHandleResponse(client, command, success, errorHandler, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PostAndHandleResponse<TCommand>(HttpClient client, TCommand command, Func<HttpResponseMessage, bool>? success, Func<HttpResponseMessage, Task>? errorHandler, CancellationToken cancellationToken) where TCommand : class
    {
        success ??= (response) => response.StatusCode == HttpStatusCode.Created;
        var route = command.GetType().GetCustomAttribute<PostAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholders(route, command);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequest.Content = JsonContent.Create(command, command.GetType());
        return await client.SendAndHandleResponse(httpRequest, success, errorHandler, cancellationToken);
    }

    public async static Task Patch<TCommand>(this HttpClient client, TCommand command, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default) where TCommand : class
    {
        success ??= (response) => response.StatusCode == HttpStatusCode.NoContent;
        var route = command.GetType().GetCustomAttribute<PatchAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholders(route, command);
        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, uri);
        httpRequest.Content = JsonContent.Create(command);
        await client.SendAndHandleResponse(httpRequest, success, errorHandler, cancellationToken);
    }

    public async static Task Delete<TCommand>(this HttpClient client, TCommand command, Func<HttpResponseMessage, bool>? success = null, Func<HttpResponseMessage, Task>? errorHandler = null, CancellationToken cancellationToken = default) where TCommand : class
    {
        success ??= (response) => response.StatusCode == HttpStatusCode.NoContent;
        var route = command.GetType().GetCustomAttribute<DeleteAttribute>()!.Route;
        var uri = PlaceholderReplacer.ReplacePlaceholders(route, command);
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, uri);
        await client.SendAndHandleResponse(httpRequest, success, errorHandler, cancellationToken);
    }
}
