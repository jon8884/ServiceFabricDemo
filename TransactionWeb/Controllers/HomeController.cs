using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Interface;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using TransactionService.Domain;

namespace TransactionWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<List<Transaction>> getTransactions()
        {
            ServiceUriBuilder builder = new ServiceUriBuilder("TransactionService");
            ITransactionService transactionService = ServiceProxy.Create<ITransactionService>(builder.ToUri(), new ServicePartitionKey(0));

            var transactions = await transactionService.GetAllSavedTransactionsAsync();
            return transactions;
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
