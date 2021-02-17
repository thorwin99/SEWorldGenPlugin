using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.ObjectBuilders;

namespace SEWorldGenPlugin.Generator.AsteroidObjects
{
    /// <summary>
    /// Abstract class that manages all instances for a custom asteroid object
    /// </summary>
    public abstract class MyAbstractAsteroidObjectProvider
    {
        /// <summary>
        /// The private instance of the admin menu creator, so that only one ever exists.
        /// </summary>
        private IMyAsteroidAdminMenuCreator m_adminMenuCreator;

        /// <summary>
        /// Loads all known data for this provider from the server,
        /// if necessary.
        /// </summary>
        public abstract void FetchDataFromServer();

        /// <summary>
        /// Returns the name of the asteroid type provided by this
        /// </summary>
        /// <returns>The typename of the asteroid object</returns>
        public abstract string GetTypeName();

        /// <summary>
        /// Whether the asteroid object provided by this class can be
        /// generated automatically at system generation. If this returns
        /// false, GetAdminMenuCreator() should return a value, so that this
        /// object is even usable.
        /// </summary>
        /// <returns>True, if it can be generated at system generation</returns>
        public abstract bool IsSystemGeneratable();

        /// <summary>
        /// Returns the shape for the given asteroid object
        /// </summary>
        /// <param name="instance">The system object instance for the corresponding asteroid</param>
        /// <returns>The asteroid shape for the asteroid object</returns>
        public abstract IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance);

        /// <summary>
        /// Returns the admin menu creator for this asteroid object type.
        /// </summary>
        /// <returns>The admin menu creator or null, if it should not be added to the plugins admin spawn and edit menu</returns>
        public IMyAsteroidAdminMenuCreator GetAdminMenuCreator()
        {
            if(m_adminMenuCreator == null)
            {
                m_adminMenuCreator = CreateAdminMenuCreatorInstance();
            }
            return m_adminMenuCreator;
        }

        /// <summary>
        /// Creates a new instance for the admin menu creator for this type and returns it.
        /// </summary>
        /// <returns>A new instance for an admin menu creator</returns>
        protected abstract IMyAsteroidAdminMenuCreator CreateAdminMenuCreatorInstance();

        /// <summary>
        /// Generates an instance of the asteroid object provided by this class.
        /// Returns the system object with corresponding display name and other values set.
        /// Saves the Instance in the provider for later use.
        /// </summary>
        /// <param name="systemIndex">Index of the object in the system</param>
        /// <param name="systemParent">The parent of this object in the system</param>
        /// <param name="objectOrbitRadius">Radius of the orbit of this instance</param>
        /// <returns>The system asteroids object</returns>
        public abstract MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius);

        /// <summary>
        /// Tries to remove the given asteroid instance from this provider
        /// </summary>
        /// <param name="systemInstance">Instance to remove</param>
        /// <returns>True, if removed</returns>
        public abstract bool RemoveInstance(MySystemAsteroids systemInstance);

        /// <summary>
        /// Tries to load the given asteroid
        /// </summary>
        public abstract bool TryLoadObject(MySystemAsteroids asteroid);

        /// <summary>
        /// Saves all the asteroid objects provided by this provider
        /// </summary>
        public abstract void OnSave();

        /// <summary>
        /// Returns the associated asteroid data provided by this class for the given asteroid object instance,
        /// if the instance is of type of this provider
        /// </summary>
        /// <param name="instance">Instance to get data for</param>
        /// <returns>The data associated or null</returns>
        public abstract object GetInstanceData(MySystemAsteroids instance);

        /// <summary>
        /// Converts an asteroid object name to a file name.
        /// </summary>
        /// <param name="objectName">The asteroid object name</param>
        /// <returns>The file name for the asteroid object</returns>
        protected string GetFileName(string objectName)
        {
            return objectName.Replace(" ", "_") + ".roid";
        }
    }
}
