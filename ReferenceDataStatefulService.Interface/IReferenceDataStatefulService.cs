using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace ReferenceDataStatefulService.Interface
{
    public interface IReferenceDataStatefulService : IService
    {
        Task<string> GetReferenceDataAsync(string dictionaryName, int id);
        Task SetReferenceDataAsync(string dictionaryName, int id, string data);
    }
}