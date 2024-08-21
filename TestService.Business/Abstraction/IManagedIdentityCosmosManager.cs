using TestService.Business.Models;

namespace TestService.Business.Abstraction
{
    public interface IManagedIdentityCosmosManager
    {
        Task<List<TestDataModel>> CosmosRead(string returnId);
        Task<string> CosmosWrite(string taxDocument);
    }
}
