using ProtoBuf;
using Sandbox.Game.Multiplayer;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using VRage;
using VRage.Game.Components;
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
        /// Handler ID for mod communication
        /// </summary>
        private const ushort HANDLER_ID = 2779;

        public override void LoadData()
        {
            if (!Sync.IsServer) return;

            MyNetUtil.RegisterMessageHandler(HANDLER_ID, PositionCheckerBytes);
        }

        protected override void UnloadData()
        {
            if (!Sync.IsServer) return;

            MyNetUtil.UnregisterMessageHandlers(HANDLER_ID);
        }

        /// <summary>
        /// Check positions of byte array, whether they are contained in asteroid fields.
        /// </summary>
        /// <param name="clientID">Client requesting lookup</param>
        /// <param name="packedData">Packed data to be checked</param>
        private void PositionCheckerBytes(ulong clientID, byte[] packedData)
        {
            List<SerializableVector3D> positions = UnpackData(packedData);

            CheckPositions(clientID, positions);
        }

        /// <summary>
        /// Check positions of unpacked Vector3D, whether they are contained in asteroid fields.
        /// </summary>
        /// <param name="clientID">Client requesting lookup</param>
        /// <param name="positions">Positions to check</param>
        private void CheckPositions(ulong clientID, List<SerializableVector3D> positions)
        {
            bool[] results = new bool[positions.Count];

            for(int i = 0; i < positions.Count; i++)
            {
                var obj = GetAsteroidObjectAt(positions[i]);

                results[i] = obj != null;
            }

            byte[] packedData = PackData(results);

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
        private List<SerializableVector3D> UnpackData(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<List<SerializableVector3D>>(data);
        }

        /// <summary>
        /// Packs boolean array into byte array
        /// </summary>
        /// <param name="data">Data to pack</param>
        /// <returns>Byte[] representation of data</returns>
        private byte[] PackData(bool[] data)
        {
            return MyAPIGateway.Utilities.SerializeToBinary(data);
        }
    }
}
