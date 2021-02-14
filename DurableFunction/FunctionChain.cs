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
using System.Linq;
using Unity;
using System;
using System.Threading;

namespace DurableFunction
{
    public class FunctionChain
    {
        private const string FunctionName = "FunctionProcessor";
        private readonly IEventProcessor _eventProcessor;
        public FunctionChain(IEventProcessor eventProcessor)
        {
            _eventProcessor = eventProcessor;
        }


        [FunctionName("FunctionChain_Chaining")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var reqData = context.GetInput<EventReq>();
            
            //function chaining
            foreach(var eventName in reqData.EventNames)
            {
                outputs.Add(await context.CallActivityAsync<string>(functionName: FunctionName, eventName));
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function_FanOutIn")]
        public async Task<List<string>> RunOrchestratorFanOutIn(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<Task<string>>();

            var reqData = context.GetInput<EventReq>();

            //function chaining
            foreach (var eventName in reqData.EventNames)
            {
                outputs.Add(context.CallActivityAsync<string>(functionName: FunctionName, eventName));
            }
            await Task.WhenAll(outputs);

            return outputs.Select(output => output.Result).ToList();
        }

        [FunctionName("Function_Monitoring")]
        public async Task RunOrchestratorMonitoring(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var jobId = context.GetInput<object>();
            int pollingInterval = 3;
            var expiryTime = context.CurrentUtcDateTime.AddMinutes(1);
            int counter = 0;
            while (context.CurrentUtcDateTime < expiryTime)
            {
                if (counter >= 5)
                {
                    // Perform an action when a condition is met.
                    log.LogInformation($"Started orchestration with ID = '{jobId}' is completed.");
                    break;
                }
                counter++;
                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(pollingInterval);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }
        }

        [FunctionName("FunctionProcessor")]
        public string ProcessEvent([ActivityTrigger] string eventName, ILogger log)
        {            
            return _eventProcessor.ProcessEvent(eventName);
        }

        [FunctionName("DurableFunction_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string requestBody = await req.Content.ReadAsStringAsync();
            EventReq data = JsonConvert.DeserializeObject<EventReq>(requestBody);
            string instanceId = string.Empty;

            if (data.EventType.Equals("Chaining", System.StringComparison.OrdinalIgnoreCase))
                instanceId = await starter.StartNewAsync("FunctionChain_Chaining", data);
            else if (data.EventType.Equals("FanOutIn", System.StringComparison.OrdinalIgnoreCase))
                instanceId = await starter.StartNewAsync("Function_FanOutIn", data);
            else
                instanceId = await starter.StartNewAsync("Function_Monitoring", (object)Guid.NewGuid());            

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}

// Post Json 
//{
//    "EventType": "Chaining", //Chaining/FanOutIn/Monitor/
//    "EventNames": ["Test1", "Test2", "Test3"]
//}