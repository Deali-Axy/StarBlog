using Microsoft.AspNetCore.Diagnostics;

namespace StarBlog.Api.Middlewares;

public static class CustomExceptionHandler {
    public static IApplicationBuilder UseCustomExceptionHandler(this WebApplication app) {
        return app.UseExceptionHandler(exceptionHandlerApp => {
            exceptionHandlerApp.Run(async context => {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

                await context.Response.WriteAsync("An exception was thrown.");

                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error is FileNotFoundException) {
                    await context.Response.WriteAsync(" The file was not found.");
                }

                if (exceptionHandlerPathFeature?.Path == "/") {
                    await context.Response.WriteAsync(" Page: Home.");
                }
            });
        });
    }
}