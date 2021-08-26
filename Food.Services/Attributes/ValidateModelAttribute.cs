using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Food.Services.Attributes
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!SkipValidation(actionContext) && !ValidateArguments(actionContext))
            {
                actionContext.Result = new BadRequestObjectResult("Аргументы не могут быть пустыми");
            }
            else if (!actionContext.ModelState.IsValid)
            {
                actionContext.Result = new BadRequestObjectResult(GetModelErrors(actionContext));
            }
        }

        private static bool SkipValidation(ActionExecutingContext actionContext)
        {
            Contract.Assert(actionContext != null);
            var actionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
            return actionDescriptor != null && (actionDescriptor.MethodInfo.GetCustomAttributes(true).Any(a => a is AllowNullAttribute)
                || actionDescriptor.ControllerTypeInfo.GetCustomAttributes(true).Any(a => a is AllowNullAttribute));
        }

        private static bool ValidateArguments(ActionExecutingContext actionContext)
        {
            Contract.Assert(actionContext != null);

            var parameters = actionContext.ActionDescriptor.Parameters;
            foreach (var item in parameters)
            {
                if (!item.ParameterType.GetCustomAttributes(true).Any(a => a is AllowNullAttribute))
                {
                    object value;
                    if (actionContext.ActionArguments.TryGetValue(item.Name, out value) && value == null)
                        return false;
                }
            }
            return true;
        }

        private static ModelError GetModelErrors(ActionExecutingContext actionContext)
        {
            var result = new ModelError
            {
                ModelState = new Dictionary<string, IEnumerable<string>>()
            };
            foreach (var item in actionContext.ModelState)
            {
                var key = item.Key.Replace("model.", string.Empty);
                result.ModelState.Add(key, item.Value.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }
    }

    public class ModelError
    {
        public string Message { get; set; }

        public Dictionary<string, IEnumerable<string>> ModelState { get; set; }
    }
}
