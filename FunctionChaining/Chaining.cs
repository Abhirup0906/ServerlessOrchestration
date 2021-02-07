using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using EventProcessor.Model;
using System.Collections.Generic;
using EventProcessor.Contract;

namespace FunctionChaining
{
    public class Chaining
    {
        private readonly IDurableClient _durableClient;
        private readonly IEventProcessor _eventProcessor;
        public Chaining(IDurableClient durableClient, IEventProcessor eventProcessor)
        {
            _durableClient = durableClient;
            _eventProcessor = eventProcessor;
        }

        [FunctionName("Chaining")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,              
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
                        
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EventReq data = JsonConvert.DeserializeObject<EventReq>(requestBody);

            var result = await _durableClient.StartNewAsync("A_Chaining", data);

            return new OkObjectResult(result);
        }

        [FunctionName("A_Chaining")]
        public async Task<IActionResult> A_Chaining(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log
            )
        {
            log.LogInformation($"************** RunOrchestrator method executing ********************");
            var reuqest = context.GetInput<EventReq>();
            List<string> result = new List<string>();
            foreach(var eventName in reuqest.EventNames)
            {
                result.Add(await _eventProcessor.ProcessEvent(eventName));
            }

            return new JsonResult(result);
        }
    }
}
