using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor.Contract
{
    public interface IEventProcessor
    {
        string ProcessEvent(string eventName);
    }
}
