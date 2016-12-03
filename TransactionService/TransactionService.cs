using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using TransactionService.Interface;
using TransactionService.Domain;
using Microsoft.ServiceFabric.Data;

namespace TransactionService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionService : StatefulService, ITransactionService
    {
        private const string TransactionDictionaryName = "transactionDictionary";
        private const string SuidDictionaryName = "suidDictionary";

        public TransactionService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var transactionDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Domain.Transaction>>(TransactionDictionaryName);
            var suidDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(SuidDictionaryName);
        }

        public async Task<List<Domain.Transaction>> GetAllSavedTransactionsAsync()
        {
            List<Domain.Transaction> results = new List<Domain.Transaction>();
            var transactionDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Domain.Transaction>>(TransactionDictionaryName);
            var suidDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(SuidDictionaryName);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var result = await suidDictionary.TryGetValueAsync(tx, "LatestSuid");
                if(result.HasValue)
                {
                    for (int i = 1; i <= result.Value; i++)
                    {
                        var transaction = await transactionDictionary.TryGetValueAsync(tx, i);
                        if(transaction.HasValue)
                        {
                            results.Add(transaction.Value);
                        }
                    }

                    ServiceEventSource.Current.ServiceMessage(
                        this.Context,
                        "Getting all the saved transactions. Found {0} transactions to return.", results.Count);
                }
                else
                {
                    ServiceEventSource.Current.ServiceMessage(
                        this.Context,
                        "There are no saved transactions to return");
                }              
            }
            return results;
        }

        public async Task SetTransactionAsync(Domain.Transaction transaction)
        {
            var transactionDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Domain.Transaction>>(TransactionDictionaryName);
            var suidDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(SuidDictionaryName);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await suidDictionary.AddOrUpdateAsync(tx, "LatestSuid", 1, (key, value) => ++value);

                var result = await suidDictionary.TryGetValueAsync(tx, "LatestSuid");

                transaction.Suid = result.Value;
                var result2 = await transactionDictionary.TryAddAsync(tx, result.Value, transaction);

                await tx.CommitAsync();
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "Request completed to add transaction for Listing: {0}, TransactionType: {1}, Price: {2}, ShareAmount {3}. Suid assigned: {4}", 
                transaction.Listing, transaction.TransactionType, transaction.Price, transaction.ShareAmount, transaction.Suid);
        }
    }
}
