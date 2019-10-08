using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEWorldGenPlugin.Networking.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public ulong Id { get; set; }

        public EventAttribute(ulong id)
        {
            Id = id;
        }
    }
}
