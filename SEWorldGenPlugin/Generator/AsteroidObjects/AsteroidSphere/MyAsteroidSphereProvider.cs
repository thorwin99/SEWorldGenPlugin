using ProtoBuf;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Provider class for a hollow sphere asteroid object
    /// </summary>
    public class MyAsteroidSphereProvider : MyAbstractAsteroidObjectProvider
    {
        /// <summary>
        /// The type name, for the system asteroid object, this class provides
        /// </summary>
        public static readonly string TYPE_NAME = "AsteroidHollowSphere";

        /// <summary>
        /// Static instance of this provider, so that there is only one provider existent ever.
        /// </summary>
        public static MyAsteroidSphereProvider Static;

        /// <summary>
        /// The dictionary contains all currently loaded asteroid spheres
        /// </summary>
        private Dictionary<string, MyAsteroidSphereData> m_loadedSpheres;

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius)
        {
            return null;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if (m_loadedSpheres.ContainsKey(instance.DisplayName))
                return MyAsteroidObjectShapeSphere.CreateFromAsteroidSphereData(m_loadedSpheres[instance.DisplayName]);
            return null;
        }

        public override string GetTypeName()
        {
            return TYPE_NAME;
        }

        public override bool IsSystemGeneratable()
        {
            return false;
        }

        public override void OnSave()
        {
            foreach(var sphere in m_loadedSpheres)
            {
                MyFileUtils.WriteXmlFileToWorld(sphere.Value, GetFileName(sphere.Key), typeof(MyAsteroidSphereProvider));
            }
        }

        public override bool RemoveInstance(MySystemAsteroids systemInstance)
        {
            MyPluginLog.Debug("Removing instance from asteroid sphere provider");

            if (systemInstance.AsteroidTypeName != GetTypeName()) return false;
            if (!m_loadedSpheres.ContainsKey(systemInstance.DisplayName)) return false;

            m_loadedSpheres.Remove(systemInstance.DisplayName);

            MyFileUtils.DeleteFileInWorldStorage(GetFileName(systemInstance.DisplayName), typeof(MyAsteroidSphereProvider));

            MyPluginLog.Debug("Successfully removed instance");

            return true;
        }

        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            if (m_loadedSpheres.ContainsKey(asteroid.DisplayName)) return false;

            if (!MyFileUtils.FileExistsInWorldStorage(GetFileName(asteroid.DisplayName), typeof(MyAsteroidSphereProvider))) return false;

            var data = MyFileUtils.ReadXmlFileFromWorld<MyAsteroidSphereData>(GetFileName(asteroid.DisplayName), typeof(MyAsteroidSphereProvider));

            m_loadedSpheres.Add(asteroid.DisplayName, data);

            return true;
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
