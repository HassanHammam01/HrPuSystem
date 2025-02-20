using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;
using System.Text.Json;
using HrPuSystem.Exceptions;
using HrPuSystem.Models;
using MyApplicationException = HrPuSystem.Exceptions.MyApplicationException;

namespace HrPuSystem.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

                var isApplicationException = ex is MyApplicationException;
                var errorMessage = isApplicationException ? ex.Message : GetUserFriendlyErrorMessage(ex);
                var statusCode = GetStatusCode(ex);

                var errorViewModel = new ErrorViewModel
                {
                    RequestId = context.TraceIdentifier,
                    ExceptionMessage = errorMessage,
                    IsDevelopment = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false,
                    IsApplicationException = isApplicationException
                };

                context.Response.StatusCode = statusCode;

                if (IsAjaxRequest(context.Request))
                {
                    var errorResponse = new { error = errorMessage, statusCode };
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
                else
                {
                    var tempData = context.RequestServices.GetService<ITempDataDictionary>();
                    if (tempData != null)
                    {
                        tempData["ErrorMessage"] = errorMessage;
                        tempData["StatusCode"] = statusCode;
                    }

                    var returnUrl = context.Request.Headers.Referer.ToString();
                    if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("/error"))
                    {
                        returnUrl = "/";
                    }

                    context.Response.Redirect($"/Home/Error?returnUrl={Uri.EscapeDataString(returnUrl)}&message={Uri.EscapeDataString(errorMessage)}&isApplicationException={isApplicationException}");
                }
            }
        }
    

        private static string GetUserFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException _ => "You do not have permission to perform this action.",
                ArgumentException _ => "Invalid input provided.",
                InvalidOperationException _ => "The requested operation cannot be completed at this time.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        private static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException _ => (int)HttpStatusCode.Forbidden,
                ArgumentException _ => (int)HttpStatusCode.BadRequest,
                InvalidOperationException _ => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json") == true;
        }
    }

    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}