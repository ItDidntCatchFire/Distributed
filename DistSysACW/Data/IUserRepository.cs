using System.Threading.Tasks;

namespace DistSysACW.Data
{
    public interface IUserRepository : IRepository<Models.User, string>
    {
        Task<Models.User> NewUserAsync(string userName);

        Task<bool> UserExistsByApiKeyAsync(string apiKey);

        Task<bool> UserExistsByUserNameAsync(string userName);

        Task<bool> UserExistsByApiKeyUserNameAsync(string apiKey, string userName);

        Task<Models.User> GetByUsernameAsync(string userName);
        
        Task<int> CountAsync();
    }
}
