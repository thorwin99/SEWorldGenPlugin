using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using VRage.Library.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidCluster
{
    /// <summary>
    /// Asteroid provider for asteroid clusters
    /// </summary>
    public class MyAsteroidClusterProvider : MyAbstractAsteroidObjectProvider<MyAsteroidClusterData>
    {
        public override MyStarSystemDesignerObjectMenu CreateStarSystemDesignerEditMenu(MySystemAsteroids instance, IMyAsteroidData data = null)
        {
            if (data == null)
                return new MyStarSystemDesigneAsteroidClusterMenu(instance, GetInstanceData(instance.Id) as MyAsteroidClusterData);
            return new MyStarSystemDesigneAsteroidClusterMenu(instance, data as MyAsteroidClusterData);
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius)
        {
            var genSettings = MySettingsSession.Static.Settings.GeneratorSettings;

            MySystemAsteroids instance = new MySystemAsteroids();
            instance.AsteroidTypeName = GetTypeName();
            instance.DisplayName = "Cluster " + (systemIndex + 1);
            instance.AsteroidSize = new MySerializableMinMax(256, 1024);

            var angle = MyRandom.Instance.GetRandomFloat(0, (float)(2 * Math.PI));
            var elevation = MyRandom.Instance.GetRandomFloat((float)Math.PI / 180f * -genSettings.SystemPlaneDeviation, (float)Math.PI / 180f * genSettings.SystemPlaneDeviation);
            instance.CenterPosition =  new Vector3D(objectOrbitRadius * Math.Cos(angle) * Math.Cos(elevation), objectOrbitRadius * Math.Sin(angle) * Math.Cos(elevation), objectOrbitRadius * Math.Sin(elevation));

            MyAsteroidClusterData data = new MyAsteroidClusterData();
            data.Size = MyRandom.Instance.Next(genSettings.PlanetSettings.PlanetSizeCap / 4, Math.Max((int)genSettings.MinMaxOrbitDistance.Min / 20, genSettings.PlanetSettings.PlanetSizeCap / 2));

            m_savedData.Add(instance.Id, data);

            return instance;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if (m_savedData.ContainsKey(instance.Id))
            {
                MyAsteroidClusterShape shape = new MyAsteroidClusterShape();
                shape.Position = instance.CenterPosition;
                shape.Size = (m_savedData[instance.Id] as MyAsteroidClusterData).Size;

                return shape;
            }
            return null;
        }

        public override IMyAsteroidData GetDefaultData(MySystemObject parent)
        {
            return new MyAsteroidClusterData();
        }

        public override MyAbstractStarSystemDesignerRenderObject GetRenderObject(MySystemAsteroids instance, IMyAsteroidData data)
        {
            if (data is MyAsteroidClusterData && instance.AsteroidTypeName == GetTypeName())
            {
                return new MyAsteroidClusterRenderer(instance, data as MyAsteroidClusterData);
            }
            return null;
        }

        public override string GetTypeName()
        {
            return "AsteroidCluster";
        }

        public override bool IsInstanceGeneratable()
        {
            return true;
        }
    }

    /// <summary>
    /// Data used for asteroid clusters
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidClusterData : IMyAsteroidData
    {
        [ProtoMember(1)]
        public double Size;
    }
}
