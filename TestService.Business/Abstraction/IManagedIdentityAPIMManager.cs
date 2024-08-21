using TestService.Business.Models;

namespace TestService.Business.Abstraction
{
    public interface IManagedIdentityApimManager
    {
        Task<APIMResponse> AquireAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
