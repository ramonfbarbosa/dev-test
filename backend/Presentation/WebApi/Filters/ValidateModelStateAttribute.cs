using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace WebApi.Filters;

public class ValidateModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .SelectMany(item => item.Value!.Errors.Select(error => new ResponseErrorItem
                {
                    Key = item.Key,
                    Value = string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "O valor informado é inválido."
                        : error.ErrorMessage
                }))
                .ToList();
            var result = new ResponseError
            {
                Message = "Ocorreram erros de validação.",
                Errors = errors
            };
            context.Result = new JsonResult(result)
            {
                StatusCode = 400
            };
        }
    }
}
