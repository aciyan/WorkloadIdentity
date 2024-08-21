namespace TestService.Business.Abstraction
{
    public interface IWorkloadIdentityRedisManager
    {
        public Task<bool> SetCacheAsync(string key, string data);
        public Task<string> GetCacheAsync(string key);
    }
}
