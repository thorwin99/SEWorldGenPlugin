using ProtoBuf;
using Sandbox.Game.Multiplayer;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRage;
using VRage.Game.Components;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Networking
{
    /// <summary>
    /// Session component managing network communication with client side mod
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ModEventHandler : MySessionComponentBase
    {
        /// <summary>
        /// Handler ID for mod communication.
        /// </summary>
        private const ushort HANDLER_ID = 2779;

        /// <summary>
        /// Handler id for generator settings requests.
        /// </summary>
        private const ushort SETTINGS_REQUEST_ID = 2780;

        public override void LoadData()
        {
            MyPluginLog.Log("Loading data for Mod communication. Enabling mod communication: " + Sync.IsServer);
            if (!Sync.IsServer) return;

            MyNetUtil.RegisterMessageHandler(HANDLER_ID, PositionCheckerBytes);
            MyNetUtil.RegisterMessageHandler(SETTINGS_REQUEST_ID, SettingsRequestHandler);
        }

        protected override void UnloadData()
        {
            MyPluginLog.Log("Unloading data for Mod communication.");
            if (!Sync.IsServer) return;

            MyNetUtil.UnregisterMessageHandlers(HANDLER_ID);
            MyNetUtil.UnregisterMessageHandlers(SETTINGS_REQUEST_ID);
        }

        /// <summary>
        /// Check positions of byte array, whether they are contained in asteroid fields.
        /// </summary>
        /// <param name="clientID">Client requesting lookup</param>
        /// <param name="packedData">Packed data to be checked</param>
        private void PositionCheckerBytes(ulong clientID, byte[] packedData)
        {
            List<SerializableVector3D> positions = UnpacLocationkData(packedData);

            MyPluginLog.Debug("Client " + clientID + "send lookup request with " + positions.Count + " positions to lookup");

            CheckPositions(clientID, positions);
        }

        /// <summary>
        /// Handler for settings requests, which returns certain generator settings to the client
        /// </summary>
        /// <param name="clientID">Client id</param>
        /// <param name="packedData">Empty packet</param>
        private void SettingsRequestHandler(ulong clientID, byte[] packedData)
        {
            MyPluginLog.Debug("Client " + clientID + " send request to retreive asteroid generator settings.");

            MyGeneratorSettings settings = new MyGeneratorSettings();
            settings.AsteroidDensity = MySettingsSession.Static.Settings.GeneratorSettings.AsteroidDensity;
            settings.UsePluginGenerator = MySettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator != AsteroidGenerationMethod.VANILLA;
            settings.WorldSize = MySettingsSession.Static.Settings.GeneratorSettings.WorldSize;

            MyPluginLog.Debug("Sending settings to " + clientID + ".");

            MyNetUtil.SendPacket(SETTINGS_REQUEST_ID, PackSettingsData(settings), clientID);
        }

        /// <summary>
        /// Check positions of unpacked Vector3D, whether they are contained in asteroid fields.
        /// </summary>
        /// <param name="clientID">Client requesting lookup</param>
        /// <param name="positions">Positions to check</param>
        private void CheckPositions(ulong clientID, List<SerializableVector3D> positions)
        {
            List<float> results = new List<float>(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                var obj = GetAsteroidObjectAt(positions[i]);

                if(obj == null)
                {
                    results[i] = -1f;
                }
                else
                {
                    MyRandom r = new MyRandom();
                    r.PushSeed(((Vector3D)positions[i]).GetHashCode());

                    results[i] = r.Next(obj.AsteroidSize.Min, obj.AsteroidSize.Max);
                }
            }

            byte[] packedData = PackResultData(results);

            MyPluginLog.Debug("Sending " + results.Count + " lookup results to client " + clientID);

            MyNetUtil.SendPacket(HANDLER_ID, packedData, clientID);
        }


        /// <summary>
        /// Tries to find a ring in the system, that contains the given position. Server only
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>The first ring found, that contains the given position.</returns>
        private MySystemAsteroids GetAsteroidObjectAt(Vector3D position)
        {
            var systemObjects = MyStarSystemGenerator.Static.StarSystem.GetAll();

            foreach (var obj in systemObjects)
            {
                if (obj.Type != MySystemObjectType.ASTEROIDS) continue;
                var asteroids = obj as MySystemAsteroids;

                if (!MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(asteroids.AsteroidTypeName, out var prov)) continue;

                IMyAsteroidObjectShape shape = prov.GetAsteroidObjectShape(asteroids);

                if (shape == null) return null;

                if (shape.Contains(position) == ContainmentType.Contains) return obj as MySystemAsteroids;
            }

            return null;
        }

        /// <summary>
        /// Unpacks byte array to Vector3Ds
        /// </summary>
        /// <param name="data">Data to unpack</param>
        /// <returns>List of Vector3Ds unpacked from data</returns>
        private List<SerializableVector3D> UnpacLocationkData(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<List<SerializableVector3D>>(data);
        }

        /// <summary>
        /// Packs boolean array into byte array
        /// </summary>
        /// <param name="data">Data to pack</param>
        /// <returns>Byte[] representation of data</returns>
        private byte[] PackResultData(List<float> data)
        {
            return MyAPIGateway.Utilities.SerializeToBinary(data);
        }

        private byte[] PackSettingsData(MyGeneratorSettings settings)
        {
            return MyAPIGateway.Utilities.SerializeToBinary(settings);
        }
    }

    /// <summary>
    /// Mod side generator settings structure
    /// </summary>
    [ProtoContract]
    public class MyGeneratorSettings
    {
        /// <summary>
        /// Whether to even generate anything
        /// </summary>
        [ProtoMember(1)]
        public bool UsePluginGenerator;

        /// <summary>
        /// Density of asteroids
        /// </summary>
        [ProtoMember(2)]
        public float AsteroidDensity;

        /// <summary>
        /// World size
        /// </summary>
        [ProtoMember(3)]
        public long WorldSize;
    }
}
