using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Core.Events
{
    public abstract class BaseEvent
    {
        protected BaseEvent(string type) 
        {
            this.Type = type;    
        }
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
    }
}
