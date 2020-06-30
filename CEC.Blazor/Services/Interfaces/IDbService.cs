using CEC.Blazor.Data;
using CEC.Blazor.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public interface IDbService<T> where T : new()
    {

        public Task<List<T>> GetRecordListAsync() => Task.FromResult(new List<T>());

        public Task<T> GetRecordAsync(int id) => Task.FromResult(new T());

        public Task<DbTaskResult> UpdateRecordAsync(T record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        public Task<DbTaskResult> AddRecordAsync(T record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        public Task<DbTaskResult> DeleteRecordAsync(int id) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message="Method not implemented" });
    }
}
