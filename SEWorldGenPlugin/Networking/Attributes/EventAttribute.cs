using System;

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
