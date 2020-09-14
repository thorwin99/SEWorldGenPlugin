using System;

namespace SEWorldGenPlugin.Networking.Attributes
{
    /// <summary>
    /// Attribute, to mark a method as a broadcast method for the
    /// PluginEventHandler, which sends this event to all clients.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BroadcastAttribute : Attribute
    {
    }
}
