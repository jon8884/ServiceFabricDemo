using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ReferenceDataStatefulService.Interface;
using Microsoft.ServiceFabric.Data;

namespace ReferenceDataStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ReferenceDataStatefulService : StatefulService, IReferenceDataStatefulService
    {
        public ReferenceDataStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Gets the current reference data from the passed in dictionary and id
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetReferenceDataAsync(string dictionaryName, int id)
        {
            if (String.IsNullOrWhiteSpace(dictionaryName))
            {
                throw new ArgumentNullException("dicationaryName");
            }

            if (id < 0)
            {
                throw new ArgumentNullException("id");
            }

            IReliableDictionary<int, string> referenceDataDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<int, string>>(dictionaryName);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var temp = await referenceDataDictionary.TryGetValueAsync(tx, id);
                if (temp.HasValue)
                {
                    return temp.Value;
                }
                else
                {
                    ServiceEventSource.Current.ServiceMessage(
                        this.Context,
                        "Getting the reference data for the passed in dictionary: {0}, and passed in ID: {1} failed",
                        dictionaryName, id);

                    throw new Exception("Reference Data does not exist in dictionary");
                }
            }
        }

        /// <summary>
        /// Sets the reference data for the passed in dictionary and id to the passed in data
        /// </summary>
        /// <returns></returns>
        public async Task SetReferenceDataAsync(string dictionaryName, int id, string data)
        {
            if (String.IsNullOrWhiteSpace(dictionaryName))
            {
                throw new ArgumentNullException("dicationaryName");
            }

            if (id < 0)
            {
                throw new ArgumentNullException("id");
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "Request for dictionary: {0}, id: {1} to be updated to {2}", dictionaryName, id, data);

            IReliableDictionary<int, string> ignitionStateDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<int, string>>(dictionaryName);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await ignitionStateDictionary.SetAsync(tx, id, data);
                await tx.CommitAsync();
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "Reference data update completed for dictionary: {0}, id: {1}, new data: {2}", dictionaryName, id, data);
        }

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
            var sampleReferenceDataDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, string>>("sampleReferenceDataDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var data1 = await sampleReferenceDataDictionary.TryGetValueAsync(tx, 1);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for sampleReferenceDataDictionary id 1: {0}",
                    data1.HasValue ? data1.Value.ToString() : "Value does not exist yet.");

                if (!data1.HasValue)
                {
                    await sampleReferenceDataDictionary.AddOrUpdateAsync(tx, 1, "Sample Data for ID = 1", (key, value) => value);
                }

                var data2 = await sampleReferenceDataDictionary.TryGetValueAsync(tx, 2);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for sampleReferenceDataDictionary id 2: {0}",
                    data2.HasValue ? data2.Value.ToString() : "Value does not exist yet.");

                if (!data2.HasValue)
                {
                    await sampleReferenceDataDictionary.AddOrUpdateAsync(tx, 2, "Sample Data for ID = 2", (key, value) => value);
                }

                var data3 = await sampleReferenceDataDictionary.TryGetValueAsync(tx, 3);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for sampleReferenceDataDictionary id 3: {0}",
                    data3.HasValue ? data3.Value.ToString() : "Value does not exist yet.");

                if (!data3.HasValue)
                {
                    await sampleReferenceDataDictionary.AddOrUpdateAsync(tx, 3, "Sample Data for ID = 3", (key, value) => value);
                }

                var data4 = await sampleReferenceDataDictionary.TryGetValueAsync(tx, 4);

                ServiceEventSource.Current.ServiceMessage(this.Context, "RunAsync is providing the current value for sampleReferenceDataDictionary id 4: {0}",
                    data4.HasValue ? data4.Value.ToString() : "Value does not exist yet.");

                if (!data4.HasValue)
                {
                    await sampleReferenceDataDictionary.AddOrUpdateAsync(tx, 4, "Sample Data for ID = 4", (key, value) => value);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }
        }
    }
}
