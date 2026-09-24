using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotificationService.Api.Filters;

public class DomainExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ArgumentException ex)
            return Task.CompletedTask;

        context.Result = new BadRequestObjectResult(new
        {
            error = ex.Message,
            parameter = ex.ParamName
        });

        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}