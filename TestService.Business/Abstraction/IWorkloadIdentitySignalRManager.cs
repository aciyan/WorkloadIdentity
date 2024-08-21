namespace TestService.Business.Abstraction
{
    public interface IWorkloadIdentitySignalRManager
    {
        Task SendAsync(string message);
    }
}
