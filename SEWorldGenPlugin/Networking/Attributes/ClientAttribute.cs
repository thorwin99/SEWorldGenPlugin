using System;

namespace SEWorldGenPlugin.Networking.Attributes
{
    /// <summary>
    /// Marks a method to be executed on the client by the PluginEventHandler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientAttribute : Attribute
    {
    }
}
