using System.Net;

namespace Totem.Infrastructure
{
    public class TestMessageResult : ApiResult
    {
        public TestMessageResult(HttpStatusCode code, string body = null) : base(code, body)
        {
            if (Body == null)
            {
                Body = "Message is valid";
            }
        }
    }
}
