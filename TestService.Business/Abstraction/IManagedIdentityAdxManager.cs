namespace TestService.Business.Abstraction
{
    public interface IManagedIdentityAdxManager
    {
        Task<List<string[]>> TestAdxConnection();
    }
}
