using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListingService.Interface
{
    public interface IListingService : IService
    {
        Task<List<string>> GetAllListingsAsync();
    }
}
