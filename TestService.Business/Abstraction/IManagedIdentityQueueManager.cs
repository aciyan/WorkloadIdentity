namespace TestService.Business.Abstraction
{
    public interface IManagedIdentityQueueManager
    {
        Task SendMessageToQueue();
    }
}
