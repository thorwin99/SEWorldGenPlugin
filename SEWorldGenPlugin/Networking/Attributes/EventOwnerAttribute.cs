using System;

namespace SEWorldGenPlugin.Networking.Attributes
{
    /// <summary>
    /// Marks a class as an EventOwner, which means this class contains
    /// methods that can be executed over network on clients or the server.
    /// It also means, all network methods in this class will be registered in the PluginEventHandler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventOwnerAttribute : Attribute
    {

    }
}
