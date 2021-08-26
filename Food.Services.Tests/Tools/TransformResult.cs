using Microsoft.AspNetCore.Mvc;

namespace Food.Services.Tests.Tools
{
    internal class TransformResult
    {
        public static T GetObject<T>(IActionResult result) where T : class
        {
            var response = result as OkObjectResult;
            return response?.Value as T;
        }

        public static T GetPrimitive<T>(IActionResult result) where T : struct
        {
            var response = result as OkObjectResult;
            return (T)(response?.Value ?? default(T));
        }
    }
}