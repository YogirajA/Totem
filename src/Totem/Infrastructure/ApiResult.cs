using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Totem.Infrastructure
{
    public class ApiResult : IActionResult
    {
        private readonly HttpStatusCode _code;

        public string Code => _code.ToString();

        public dynamic Body { get; set; }

        public ApiResult(HttpStatusCode code, string body = null)
        {
            _code = code;
            Body = body;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(this);

            if (Code == "NotFound")
            {
                objectResult.StatusCode = StatusCodes.Status404NotFound;
            }
            else if (Code == "OK")
            {
                objectResult.StatusCode = StatusCodes.Status200OK;
            }
            else if (Code == "BadRequest")
            {
                objectResult.StatusCode = StatusCodes.Status400BadRequest;
            }

            await objectResult.ExecuteResultAsync(context);
        }
    }
}
