using EventProcessor.Contract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor.Service
{
    public class EventProcessor : IEventProcessor
    {
        public string ProcessEvent(string eventName)
        {
            return $"{eventName} has been successfully processed";
        }
    }
}
