using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Provider class for a hollow sphere asteroid object
    /// </summary>
    public class MyAsteroidSphereProvider : MyAbstractAsteroidObjectProvider
    {
        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius)
        {
            throw new NotImplementedException();
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            throw new NotImplementedException();
        }

        public override string GetTypeName()
        {
            throw new NotImplementedException();
        }

        public override bool IsSystemGeneratable()
        {
            throw new NotImplementedException();
        }

        public override void OnSave()
        {
            throw new NotImplementedException();
        }

        public override bool RemoveInstance(MySystemAsteroids systemInstance)
        {
            throw new NotImplementedException();
        }

        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            throw new NotImplementedException();
        }

        protected override IMyAsteroidAdminMenuCreator CreateAdminMenuCreatorInstance()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class for additional data to save an asteroid object hollow sphere
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidSphereData
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
