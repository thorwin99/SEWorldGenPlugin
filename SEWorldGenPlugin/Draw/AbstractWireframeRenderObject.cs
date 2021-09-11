namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// Abstract class to provide base functionality for wireframe rendered objects
    /// </summary>
    public abstract class AbstractWireframeRenderObject : IRenderObject
    {
        /// <summary>
        /// The thickness of the lines drawn by this render object in world units.
        /// </summary>
        public float LineThickness;

        public AbstractWireframeRenderObject(float lineThickness)
        {
            LineThickness = lineThickness;
        }

        public abstract void Draw();
    }
}
