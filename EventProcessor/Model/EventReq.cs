using System;
using System.Collections.Generic;
using System.Text;

namespace EventProcessor.Model
{
    public class EventReq
    {
        public string EventType { get; set; }
        public IEnumerable<string> EventNames { get; set; }
    }
}
