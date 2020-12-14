namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// Abstract base class for object builders used
    /// for configuration files. it will verify all members
    /// on constructor call and provides a method to copy
    /// it into a new instance.
    /// </summary>
    public abstract class MyAbstractConfigObjectBuilder
    {

        public MyAbstractConfigObjectBuilder()
        {
            Verify();
        }

        /// <summary>
        /// Copies this object and returns a new
        /// instance with the same member values.
        /// </summary>
        /// <returns>The copy of this object</returns>
        public abstract MyAbstractConfigObjectBuilder copy();

        /// <summary>
        /// This method verifies members of this class, and resets them
        /// to default if needed.
        /// </summary>
        public abstract void Verify();
    }
}
