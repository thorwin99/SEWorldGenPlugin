using SEWorldGenPlugin.Draw;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// This class is used to render the star system in the star system designer
    /// </summary>
    public class MyStarSystemRenderer : IRenderObject
    {
        /// <summary>
        /// A dictionary of all render objects for the system objects
        /// </summary>
        private Dictionary<Guid, IRenderObject> m_systemRenderObjects;

        public MyStarSystemRenderer()
        {
            m_systemRenderObjects = new Dictionary<Guid, IRenderObject>();
        }

        /// <summary>
        /// Adds a new object to the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object while the
        /// <paramref name="obj"/> is a render object representing the system object.
        /// Only one render object can be added for a given system object.
        /// </summary>
        /// <param name="id">Id of the system object</param>
        /// <param name="obj">Render object to visualize the system object</param>
        public void AddObject(Guid id, IRenderObject obj)
        {
            if (m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects.Add(id, obj);
        }

        /// <summary>
        /// Removes an object from the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object to remove
        /// </summary>
        /// <param name="id">Id of the system object</param>
        public void RemoveObject(Guid id)
        {
            if (!m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects.Remove(id);
        }

        /// <summary>
        /// Updates an existing object in the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object while the
        /// </summary>
        /// <param name="id">Id of the system object</param>
        /// <param name="obj">Render object to visualize the system object</param>
        public void UpdateRenderObject(Guid id, IRenderObject obj)
        {
            if (!m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects[id] = obj;
        }

        /// <summary>
        /// Clears all system objects from the render list.
        /// </summary>
        public void ClearRenderList()
        {
            m_systemRenderObjects.Clear();
        }

        public void Draw()
        {
            foreach(var entry in m_systemRenderObjects)
            {
                entry.Value.Draw();
            }
        }
    }
}
