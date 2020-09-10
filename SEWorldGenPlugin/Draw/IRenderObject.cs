namespace SEWorldGenPlugin.Draw
{
    /// <summary>
    /// An interface that provides a Draw method for
    /// all classes that can be drawn in the gameworld, such as simple spheres.
    /// </summary>
    public interface IRenderObject
    {
        /// <summary>
        /// The draw method used to draw items into the world.
        /// </summary>
        void Draw();
    }
}
