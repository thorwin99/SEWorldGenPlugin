using System;

namespace SEWorldGenPlugin.Networking.Attributes
{
    /// <summary>
    /// Marks a method as a NetworkEvent to be registered in the PluginEventHandler.
    /// The method also needs at least the broadcast, client or server attribute, to actually get
    /// executed. The containing class needs to have the EventOwner attribute to actually register this
    /// method in the PluginEventHandler
    /// </summary>
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
