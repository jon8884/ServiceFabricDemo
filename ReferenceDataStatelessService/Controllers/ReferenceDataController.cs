using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ReferenceDataStatefulService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ReferenceDataStatelessService.Controllers
{
    public class ReferenceDataController : ApiController
    {
        // GET api/referencedata/dictionaryName/id
        [Route("api/referenceData/{dictionaryName}/{id}")]
        public async Task<HttpResponseMessage> Get(string dictionaryName, int id)
        {
            if (string.IsNullOrEmpty(dictionaryName))
            {
                ServiceEventSource.Current.Message("Please specify a reference data dictionary to access data");
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            if (id < 1)
            {
                ServiceEventSource.Current.Message("Please pass in an appropriate id value");
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            ServiceUriBuilder builder = new ServiceUriBuilder("ReferenceDataStatefulService");
            IReferenceDataStatefulService referenceDataService = ServiceProxy.Create<IReferenceDataStatefulService>(builder.ToUri(), new ServicePartitionKey(0));

            var referenceData = await referenceDataService.GetReferenceDataAsync(dictionaryName, id);
            return this.Request.CreateResponse(HttpStatusCode.Accepted, referenceData);
        }

        // GET api/referencedata/dictionaryName/id/referenceData
        [Route("api/referenceData/{dictionaryName}/{id}/{referenceData}")]
        public async Task<HttpResponseMessage> Post(string dictionaryName, int id, string referenceData)
        {
            if (string.IsNullOrEmpty(dictionaryName))
            {
                ServiceEventSource.Current.Message("Please specify a reference data dictionary to access data");
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            if (id < 1)
            {
                ServiceEventSource.Current.Message("Please pass in an appropriate id value");
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                ServiceUriBuilder builder = new ServiceUriBuilder("ReferenceDataStatefulService");
                IReferenceDataStatefulService referenceDataService = ServiceProxy.Create<IReferenceDataStatefulService>(builder.ToUri(), new ServicePartitionKey(0));

                await referenceDataService.SetReferenceDataAsync(dictionaryName, id, referenceData);

                return this.Request.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
