using Microsoft.AspNetCore.Diagnostics;

namespace EmployeeManagement.Server;

public static class ApiErrorResponses
{
    // For requests under /api, keep error responses as JSON problem details:
    // - stops the HTML "not found" re-execute from replacing API errors with a web page;
    // - fills in a problem details body for errors the framework returns with no body,
    //   such as a route that doesn't match (404) or an unsupported HTTP method (405).
    // Must be registered after UseStatusCodePagesWithReExecute.
    public static IApplicationBuilder UseApiErrorResponses(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await next(context);
                return;
            }

            if (context.Features.Get<IStatusCodePagesFeature>() is { } statusCodePages)
            {
                statusCodePages.Enabled = false;
            }

            await next(context);

            var response = context.Response;
            if (response.StatusCode >= 400 && !response.HasStarted
                && response.ContentLength is null && string.IsNullOrEmpty(response.ContentType))
            {
                var problemDetails = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetails.WriteAsync(new ProblemDetailsContext { HttpContext = context });
            }
        });
}