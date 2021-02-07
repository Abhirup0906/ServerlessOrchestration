using EventProcessor.Contract;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventProcessor
{
    public static class EventProcessorBuilder
    {
        public static void Register(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IEventProcessor, Service.EventProcessor>();
        }
    }
}
