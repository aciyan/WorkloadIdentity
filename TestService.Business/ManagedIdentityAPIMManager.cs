using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using TestService.Business.Abstraction;
using TestService.Business.Models;
using System.Text;

namespace TestService.Business
{
    public class ManagedIdentityApimManager : IManagedIdentityApimManager
    {
        #region Private Properties

        // The token credential
        private readonly TokenCredential _tokenCredential;
        private readonly ILogger _logger = Logger.CreateLogger(nameof(ManagedIdentityApimManager));

        #endregion

        #region Public Constructor

        public ManagedIdentityApimManager(TokenCredential credential)
        {
            _tokenCredential = credential;
        }

        #endregion

        #region Public Methods

        public async Task<APIMResponse> AquireAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
                var token = await _tokenCredential.GetTokenAsync(tokenContext, cancellationToken);

                // Create an HttpClient instance
                using var httpClient = new HttpClient();
                // Set the authorization header with the access token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                httpClient.DefaultRequestHeaders.Add("Consumer", "TEST");
                // Define the URL you want to connect to
                string uriString = "https://test-dev.net/client-transactions";
                string host = "dev.test.net";
                string request = "{phoneSearch{phone:{phoneNumber:\"8169959452\"}}{augment{ucId}failure{failureId failureMessage failureCode}}}";

                var content = new StringContent(request, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentLength = request.Length;
                var httpRequest = new HttpRequestMessage()
                {
                    Content = content,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(uriString)
                };
                httpRequest.Headers.Host = host;

                _logger.LogInformation($"TestService-ManagedIdentity: Request: {httpRequest}");
                // Send a GET request to the URL
                HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
                HttpResponseMessage response2 = await httpClient.PostAsync(uriString, content);
                _logger.LogInformation("TestService-ManagedIdentity: Sent Http request");

                string responseBody = null;
                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                var apimResponse = new APIMResponse()
                {
                    StatusCode = response.StatusCode,
                    ResponseBody = responseBody,
                    Token = token.Token,
                    Response = response2
                };

                return apimResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService-ManageIdentity");
                return null;
            }
            

        }
        #endregion
    }
}
