using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Food.Services.ExceptionHandling
{
    public class InternalServerError : IActionResult
    {
        public InternalServerError(string content = null)
        {
            Content = content;
        }

        public InternalServerError() { }
        public string Content { get; set; }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(Content)
            };
            return Task.FromResult(response);
        }
    }
}
