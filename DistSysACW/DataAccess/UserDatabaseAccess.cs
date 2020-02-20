using DistSysACW.Data;
using DistSysACW.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application

namespace DistSysACW.DataAccess
{
    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;

        public UserRepository(UserContext context)
        {
            _context = context;
        }

        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 

        public async Task<User> NewUserAsync(string userName)
        {
            var user = new User()
            { 
                UserName = userName,
                Role = User.Roles.User
            };

            await AddAsync(user);

            return user;
        }

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members
        public async Task<bool> UserExistsByApiKeyAsync(string apiKey)
            => await GetByIdAsync(apiKey) != null;
        
        public async Task<bool> UserExistsByApiKeyUserNameAsync(string apiKey, string userName)
        {
            var user = _context.Users.FirstOrDefault(a => a.ApiKey == apiKey && a.UserName == userName);

            return user != null;
        }

        public async Task<IEnumerable<User>> ListAsync()
            => _context.Users.ToList();

        public async Task<User> GetByIdAsync(string Id)
            => await _context.Users.FindAsync(Id);

        public async Task<User> AddAsync(User type)
        {
            type.ApiKey = Guid.NewGuid().ToString();
            _context.Users.Add(type);
            return type;
        }

        public async Task DeleteAsync(string Id)
        {
            _context.Remove(GetByIdAsync(Id));
        }

        public async Task UpdateAsync(User type)
        {
            _context.Entry(type).State = EntityState.Modified;
        }

        public async Task SaveAsync()
            => await _context.SaveChangesAsync();

        public async Task<bool> UserExistsByUserNameAsync(string userName)
        {
            var user = _context.Users.FirstOrDefault(a => a.UserName == userName);
            return user != null;
        }

        public async Task<int> CountAsync()
            => _context.Users.Count();
        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
