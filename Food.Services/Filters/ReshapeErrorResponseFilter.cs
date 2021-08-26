using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Food.Services.Filters
{
    /// <summary>
    /// Фильтр конвертирующий ошибочные ответы содержащие единственную
    /// строку с ошибкой в объект с типом след. формы: { message: "@msg" }.
    /// 
    /// Этот фильтр это временное решение проблемы того что ошибочные ответы
    /// у нас это суммарный тип (https://en.wikipedia.org/wiki/Tagged_union).
    /// Со стороны C# нет поддержки таких типов и со стороны json.net тоже, поэтому
    /// этот фильтр приводит проблемные варианты к общему знаменателю.
    /// </summary>
    public class ReshapeErrorResponseFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is BadRequestObjectResult result && result.Value is string msg) {
                context.Result = new BadRequestObjectResult(new { Message = msg });
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
