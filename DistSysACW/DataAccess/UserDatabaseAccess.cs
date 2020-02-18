using DistSysACW.Data;
using DistSysACW.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DistSysACW.DataAccess
{
    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public class UserDatabaseAccess
    {
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 

        public static async Task<User> NewUserAsync(string userName, UserContext context)
        {
            var user = new User()
            {
                ApiKey = Guid.NewGuid().ToString(),
                UserName = userName,
                Role = User.Roles.User
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public static async Task<bool> UserExistsByUserNameAsync(string userName, UserContext context)
        {
            var user = context.Users.FirstOrDefault(a => a.UserName == userName);
            return user != null;
        }

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members
        public static async Task<bool> UserExistsByApiKeyAsync(string apiKey, UserContext context)
            => GetUserByApiKeyAsync(apiKey, context) != null;

        public static async Task<bool> UserExistsByApiKeyAsync(string apiKey, string userName, UserContext context)
        {
            var user = context.Users.FirstOrDefault(a => a.ApiKey == apiKey && a.UserName == userName);

            return user != null;
        }

        public static async Task<User> GetUserByApiKeyAsync(string apiKey, UserContext context)
        {
            var user = context.Users.FirstOrDefault(a => a.ApiKey == apiKey);
            return user;
        }

        public static async Task DeleteUserAsync(string apiKey, UserContext context)
        {
            context.Remove(GetUserByApiKeyAsync(apiKey, context));
            await context.SaveChangesAsync();
        }

        #endregion
    }
}
