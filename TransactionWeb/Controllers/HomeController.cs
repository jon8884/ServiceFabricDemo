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

        [HttpGet]
        public async Task<JsonResult> GetTransactions()
        {
            ServiceUriBuilder builder = new ServiceUriBuilder("TransactionService");
            ITransactionService transactionService = ServiceProxy.Create<ITransactionService>(builder.ToUri(), new ServicePartitionKey(0));

            var transactions = await transactionService.GetAllSavedTransactionsAsync();
            return Json(transactions);
        }

        [HttpPost]
        public async void SaveTransaction(string listing, string transactionType, decimal price, int shareAmount)
        {
            ServiceUriBuilder builder = new ServiceUriBuilder("TransactionService");
            ITransactionService transactionService = ServiceProxy.Create<ITransactionService>(builder.ToUri(), new ServicePartitionKey(0));

            TransactionType transactionTypeEnum;
            Enum.TryParse(transactionType, out transactionTypeEnum);

            var transaction = new Transaction()
            {
                Listing = listing,
                TransactionType = transactionTypeEnum,
                Price = price,
                ShareAmount = shareAmount
            };

            await transactionService.SetTransactionAsync(transaction);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
