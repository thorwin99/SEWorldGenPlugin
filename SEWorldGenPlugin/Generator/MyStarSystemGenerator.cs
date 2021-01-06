using SEWorldGenPlugin.Networking.Attributes;
using System.Collections.Generic;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Session component that generates the solar system data for the solar system of
    /// the current game session / world, and provides networking functions to
    /// manipulate it clientside. Is a singleton class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 600)]
    [EventOwner]
    public partial class MyStarSystemGenerator
    {
        /// <summary>
        /// File name for the file containing the system data for the world
        /// </summary>
        private readonly string STORAGE_FILE = "SystemData.xml";

        /// <summary>
        /// List of all vanilla planets, to allow users to exclude them from world generation
        /// </summary>
        private readonly List<string> VANILLA_PLANETS = new List<string> { "Alien", "EarthLike", "Europa", "Mars", "Moon", "Pertam", "Titan", "Triton" };


    }
}
