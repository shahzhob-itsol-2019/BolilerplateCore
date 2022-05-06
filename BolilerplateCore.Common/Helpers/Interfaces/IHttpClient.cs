using BoilerplateCore.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Common.Helpers.Interfaces
{
    public interface IHttpClient
    {
        Task SetBearerToken(string token);
        Task<LoginResponseModel> TokenAsync(string action, object data);
        Task<BaseModel<T>> GetAsync<T>(string action);
        Task<BaseModel<T>> PostAsync<T>(string action);
        Task<BaseModel<T>> PostAsync<T>(string action, object data);
        Task<BaseModel<T>> PutAsync<T>(string action, object data);
        Task<BaseModel<T>> DeleteAsync<T>(string action);
    }
}
