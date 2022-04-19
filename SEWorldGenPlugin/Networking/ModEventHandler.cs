using ProtoBuf;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.Generator.ProceduralGeneration;
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
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ModEventHandler : MySessionComponentBase
    {
        private const ushort HANDLER_ID = 2779;

        public static ModEventHandler Instance;

        public override void LoadData()
        {
            MyNetUtil.RegisterMessageHandler(HANDLER_ID, PositionCheckerBytes);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
        }

        private void PositionCheckerBytes(ulong clientID, byte[] packedData)
        {
            List<SerializableVector3D> positions = UnpackData(packedData);

            CheckPositions(clientID, positions);
        }

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

        private List<SerializableVector3D> UnpackData(byte[] data)
        {
            List<SerializableVector3D> pos;

            using (var ms = new MemoryStream(data))
            {
                pos = Serializer.DeserializeWithLengthPrefix<List<SerializableVector3D>>(ms, PrefixStyle.Base128);

                return pos;
            }
        }

        private byte[] PackData(bool[] data)
        {
            BitArray bits = new BitArray(data);

            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, bits, PrefixStyle.Base128);
                return ms.ToArray();
            }
        }
    }
}
