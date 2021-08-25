using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Provider class for a hollow sphere asteroid object
    /// </summary>
    public class MyAsteroidSphereProvider : MyAbstractAsteroidObjectProvider<MyAsteroidSphereData>
    {
        /// <summary>
        /// The type name, for the system asteroid object, this class provides
        /// </summary>
        public static readonly string TYPE_NAME = "AsteroidHollowSphere";

        /// <summary>
        /// Static instance of this provider, so that there is only one provider existent ever.
        /// </summary>
        public static MyAsteroidSphereProvider Static;

        public MyAsteroidSphereProvider()
        {
            Static = this;
        }

        public override MyStarSystemDesignerObjectMenu CreateStarSystemDesignerEditMenu(MySystemAsteroids instance)
        {
            return new MyStarSystemDesignerAsteroidSphereMenu(instance, GetInstanceData(instance.Id) as MyAsteroidSphereData);
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius)
        {
            return null;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if (m_savedData.ContainsKey(instance.Id))
                return MyAsteroidObjectShapeSphere.CreateFromAsteroidSphereData(m_savedData[instance.Id] as MyAsteroidSphereData);
            return null;
        }

        public override string GetTypeName()
        {
            return TYPE_NAME;
        }

        public override bool IsInstanceGeneratable()
        {
            return false;
        }
    }

    /// <summary>
    /// Class for additional data to save an asteroid object hollow sphere
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidSphereData : IMyAsteroidData
    {
        /// <summary>
        /// The center of the hollow sphere asteroids object
        /// </summary>
        [ProtoMember(1)]
        public Vector3D Center;

        /// <summary>
        /// The inner radius for the hollow sphere of asteroids
        /// </summary>
        [ProtoMember(2)]
        public double InnerRadius;

        /// <summary>
        /// The outer radius for the hollow sphere asteroids
        /// </summary>
        [ProtoMember(3)]
        public double OuterRadius;
    }
}
