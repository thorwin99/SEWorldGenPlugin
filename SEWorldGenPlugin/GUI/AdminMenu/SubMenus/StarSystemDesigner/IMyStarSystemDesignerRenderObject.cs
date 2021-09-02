using SEWorldGenPlugin.Draw;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    public interface IMyStarSystemDesignerRenderObject : IRenderObject
    {
        /// <summary>
        /// Updates the render object properties to reflect changes in the scene.
        /// <paramref name="CameraFocusLength"/> is used to determine the new Line thickness of this rendered object
        /// </summary>
        /// <param name="CameraFocusLength">The distance of the camera to its focus point.</param>
        void Update(double CameraFocusLength);
    }
}
