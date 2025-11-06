namespace OwaspTool.Services
{
    public interface IProjectUserSyncService
    {
        Task CreateUserInMainDbAsync(string email);
    }
}
