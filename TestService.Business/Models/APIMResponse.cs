using System.Net;

namespace TestService.Business.Models
{
    public class APIMResponse
    {
        public string Token { get; set; }
        public string ResponseBody { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseMessage Response { get; set; }
    }
}
