using DidActivation.Domain.Exceptions;
using Polly.CircuitBreaker;
using System.Net;
using System.Text.Json;

namespace DidActivation.Api.Middleware
{
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.ErrorCode, ex.Message);
            }
            catch (BrokenCircuitException)
            {
                await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable, "PARTNER_UNAVAILABLE", "Partner integration temporarily unavailable.");
            }
            catch (DomainException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.UnprocessableEntity, ex.ErrorCode, ex.Message);
            }
            catch (OperationCanceledException)
            {
                if (!context.RequestAborted.IsCancellationRequested)
                    await WriteErrorAsync(context, HttpStatusCode.RequestTimeout, "REQUEST_TIMEOUT", "Request timed out.");
            }
            catch (Exception)
            {
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "UNEXPECTED_ERROR", "An unexpected error occurred.");
            }
        }

        private static Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string errorCode, string message)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                error = errorCode,
                message
            });

            return context.Response.WriteAsync(body);
        }
    }
}

