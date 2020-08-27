
namespace SEWorldGenPlugin.Utilities
{
    static class StringExtension
    {

        public static string SetProperty<T>(this string s, string propertyName, T value)
        {
            return s.Replace("[" + propertyName + "]", value.ToString());
        }
    }
}
