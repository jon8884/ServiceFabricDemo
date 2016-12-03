using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.Domain;

namespace TransactionService.Interface
{
    public interface ITransactionService : IService
    {
        Task<List<Transaction>> GetAllSavedTransactionsAsync();
        Task SetTransactionAsync(Transaction transaction);
    }
}
