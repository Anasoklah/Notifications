using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotificationService.Api.Filters;

public class ValidationExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ValidationException ex)
            return Task.CompletedTask;

        context.Result = new BadRequestObjectResult(
            ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}