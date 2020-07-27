using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Totem.Features.Contracts;
using Totem.Infrastructure;

namespace Totem.Features.API
{
    public class ApiController : Controller
    {
        private readonly IMediator _mediator;

        public ApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: API endpoint for automating message testing
        [HttpPost]
        public async Task<TestMessageResult> TestMessage([FromBody] TestMessage.Command command)
        {
            try
            {
                var result = await _mediator.Send(command);
                
                if (result.IsValid)
                {
                    return new TestMessageResult(HttpStatusCode.OK);
                }

                var messageErrors = result.MessageErrors;
                messageErrors.AddRange(result.Warnings);
                var inlined = string.Join(" ", messageErrors);
                
                return new TestMessageResult(HttpStatusCode.BadRequest, $"Message does not match contract: {inlined}");
            }
            catch
            {
                return new TestMessageResult(HttpStatusCode.BadRequest, "Unable to test message; confirm that your request message is valid JSON.");
            }
        }

        [HttpPost]
        public async Task<ApiResult> GenerateSampleData([FromBody] SampleData.Command command)
        {
            try
            {
                var result = await _mediator.Send(command);

                return new ApiResult(HttpStatusCode.OK, result.SampleData);
            }
            catch
            {
                return new ApiResult(HttpStatusCode.NotFound, "Unable to generate sample data for the requested contract.");
            }
        }

        [HttpPost]
        public async Task<ApiResult> GetContractDetails([FromBody] ContractDetails.Command command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return new ApiResult(HttpStatusCode.OK) { Body = ContractDisplay.ContractDetails(result.Contract) };
            }
            catch
            {
                return new ApiResult(HttpStatusCode.NotFound, "Unable to find the contract specified.");
            }
        }
    }
}
