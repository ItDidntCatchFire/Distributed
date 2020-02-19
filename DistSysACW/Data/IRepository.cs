using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistSysACW.Data
{
    public interface IRepository<T, U> : IDisposable where T : Models.IModel
    {
        //Lovely CRUD
        Task<IEnumerable<T>> ListAsync();
        Task<T> GetByIdAsync(U Id);
        Task<T> AddAsync(T type);
        Task DeleteAsync(U Id);
        Task UpdateAsync(T type);
        Task SaveAsync();
    }
}
