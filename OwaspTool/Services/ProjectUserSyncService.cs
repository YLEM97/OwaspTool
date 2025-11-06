using OwaspTool.DAL;
using OwaspTool.Models.Database;

namespace OwaspTool.Services
{
    public class ProjectUserSyncService : IProjectUserSyncService
    {
        private readonly OwaspToolContext _context;

        public ProjectUserSyncService(OwaspToolContext context)
        {
            _context = context;
        }

        public async Task CreateUserInMainDbAsync(string email)
        {
            var u = new User
            {
                UserID = Guid.NewGuid(),
                Email = email
            };

            _context.Users.Add(u);
            await _context.SaveChangesAsync();
        }

    }
}
