using System;

namespace SEWorldGenPlugin.Networking.Attributes
{
    /// <summary>
    /// Marks a method to be executed on the server by the PluginEventHandler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerAttribute : Attribute
    {
    }
}
