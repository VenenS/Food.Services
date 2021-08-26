using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Common.Serilog.ActionFilters
{
    /// <summary>
    /// Фильтр для обогащения логгинг контекста данными по
    /// выполняемому mvc экшену.
    /// </summary>
    public class MvcLoggingEnricher : IActionFilter
    {
        private readonly IDiagnosticContext _context;

        public MvcLoggingEnricher(IDiagnosticContext context)
        {
            _context = context;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _context.Set("ActionName", context.ActionDescriptor.DisplayName);
            _context.Set("ActionId", context.ActionDescriptor.Id);
        }
    }
}
