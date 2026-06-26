using FluentValidation;
using FluentValidation.Results;

namespace TaskSystem.API.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var validators = context.RequestServices.GetServices<IValidator>().ToList();

        if (validators.Any())
        {
            foreach (var validator in validators)
            {
                var model = context.Items["__RequestBody"];
                if (model == null)
                    continue;

                ValidationResult result = await validator.ValidateAsync(
                    new ValidationContext<object>(model)
                );

                if (!result.IsValid)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(
                        new { success = false, errors = result.Errors.Select(e => e.ErrorMessage) }
                    );
                    return;
                }
            }
        }

        await _next(context);
    }
}
