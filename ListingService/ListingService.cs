using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ListingService.Interface;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace ListingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ListingService : StatefulService, IListingService
    {
        private const string ListingDictionaryName = "listingDictionary";

        public ListingService(StatefulServiceContext context)
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
            var listingDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, string>>(ListingDictionaryName);
            using (var tx = this.StateManager.CreateTransaction())
            {
                var listing1 = await listingDictionary.TryGetValueAsync(tx, 1);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for listingDictionary id 1: {0}",
                    listing1.HasValue ? listing1.Value.ToString() : "Value does not exist yet.");

                if (!listing1.HasValue)
                {
                    await listingDictionary.AddOrUpdateAsync(tx, 1, "Amazon", (key, value) => value);
                }

                var listing2 = await listingDictionary.TryGetValueAsync(tx, 2);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for listingDictionary id 2: {0}",
                    listing2.HasValue ? listing2.Value.ToString() : "Value does not exist yet.");

                if (!listing2.HasValue)
                {
                    await listingDictionary.AddOrUpdateAsync(tx, 2, "Microsoft", (key, value) => value);
                }

                var listing3 = await listingDictionary.TryGetValueAsync(tx, 3);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for listingDictionary id 3: {0}",
                    listing3.HasValue ? listing3.Value.ToString() : "Value does not exist yet.");

                if (!listing3.HasValue)
                {
                    await listingDictionary.AddOrUpdateAsync(tx, 3, "Apple", (key, value) => value);
                }

                var listing4 = await listingDictionary.TryGetValueAsync(tx, 4);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for listingDictionary id 4: {0}",
                    listing4.HasValue ? listing4.Value.ToString() : "Value does not exist yet.");

                if (!listing4.HasValue)
                {
                    await listingDictionary.AddOrUpdateAsync(tx, 4, "Symantec", (key, value) => value);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }
        }

        public async Task<List<string>> GetAllListingsAsync()
        {
            List<string> listings = new List<string>();
            var listingDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, string>>(ListingDictionaryName);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var listingCount = await listingDictionary.GetCountAsync(tx);

                for(var i = 0; i < listingCount; i++)
                { 
                    var listing = await listingDictionary.TryGetValueAsync(tx, i);
                    if (listing.HasValue)
                    {
                        listings.Add(listing.Value);
                    }
                }

                ServiceEventSource.Current.ServiceMessage(
                    this.Context,
                    "Getting all listings. Found {0} listings to return.", listings.Count);
            }

            return listings;
        }
    }
}
