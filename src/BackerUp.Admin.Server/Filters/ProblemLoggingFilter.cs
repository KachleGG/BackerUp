using BackerUp.Admin.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackerUp.Admin.Server.Filters;

public class ProblemLoggingFilter : IAsyncActionFilter, IAsyncExceptionFilter
{
    private readonly ProblemLogService _problemLogService;

    public ProblemLoggingFilter(ProblemLogService problemLogService)
    {
        _problemLogService = problemLogService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ActionExecutedContext executedContext = await next();

        if (executedContext.Exception != null || executedContext.Canceled)
        {
            return;
        }

        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode.HasValue && objectResult.StatusCode.Value >= 400)
        {
            _problemLogService.LogWarning(BuildDescription(context, objectResult.StatusCode.Value, objectResult.Value));
            return;
        }

        if (executedContext.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 400)
        {
            _problemLogService.LogWarning(BuildDescription(context, statusCodeResult.StatusCode, null));
        }
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        _problemLogService.LogException(BuildScope(context), context.Exception);
        return Task.CompletedTask;
    }

    private static string BuildDescription(ActionExecutingContext context, int statusCode, object? resultValue)
    {
        string scope = BuildScope(context);
        string detail = resultValue switch
        {
            string text when !string.IsNullOrWhiteSpace(text) => text,
            ProblemDetails problemDetails when !string.IsNullOrWhiteSpace(problemDetails.Detail) => problemDetails.Detail,
            null => string.Empty,
            _ => resultValue.ToString() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(detail))
        {
            return $"{scope} returned HTTP {statusCode}.";
        }

        return $"{scope} returned HTTP {statusCode}: {detail}";
    }

    private static string BuildScope(FilterContext context)
    {
        string controller = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerName) ? controllerName ?? "UnknownController" : "UnknownController";
        string action = context.ActionDescriptor.RouteValues.TryGetValue("action", out var actionName) ? actionName ?? "UnknownAction" : "UnknownAction";
        return $"{controller}.{action}";
    }
}