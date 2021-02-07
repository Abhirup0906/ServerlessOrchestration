using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using EventProcessor.Contract;
using EventProcessor.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity;

namespace DurableFunction
{
    public class DurableFunction
    {
        private const string FunctionName = "DurableFunc";
        private readonly IEventProcessor _eventProcessor;
        public DurableFunction(IEventProcessor eventProcessor)
        {
            _eventProcessor = eventProcessor;
        }


        [FunctionName("DurableFunc_Chaining")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var reqData = context.GetInput<EventReq>();
            

            foreach(var eventName in reqData.EventNames)
            {
                outputs.Add(await context.CallActivityAsync<string>(functionName: FunctionName, eventName));
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurableFunc")]
        public string SayHello([ActivityTrigger] string eventName, ILogger log)
        {            
            return _eventProcessor.ProcessEvent(eventName);
        }

        [FunctionName("DurableFunc_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string requestBody = await req.Content.ReadAsStringAsync();
            EventReq data = JsonConvert.DeserializeObject<EventReq>(requestBody);

            string instanceId = await starter.StartNewAsync("DurableFunc_Chaining", data);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}