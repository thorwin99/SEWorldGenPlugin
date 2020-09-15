namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Extension methods for string class
    /// </summary>
    static class StringExtension
    {
        /// <summary>
        /// Sets a property in a string. A property is a substring [propertyName].
        /// </summary>
        /// <typeparam name="T">Type of the object to set the property to</typeparam>
        /// <param name="s">String to execute this method on</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        /// <returns>A string where the property is replaced to the value</returns>
        /// <example>
        /// <code>"Hello [name]".SetProperty("name", "Peter");// returns "Hello Peter"</code>
        /// </example>
        public static string SetProperty<T>(this string s, string propertyName, T value)
        {
            return s.Replace("[" + propertyName + "]", value.ToString());
        }
    }
}
