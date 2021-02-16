using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.Generator
{
    public partial class MyStarSystemGenerator
    {
        /// <summary>
        /// Generates a name for the planet based on the naming scheme
        /// defined in the global settings file
        /// </summary>
        /// <param name="planetIndex">Index of the planet in the system. Zero based</param>
        /// <param name="subtypeId">Type of the planet</param>
        /// <returns>Name for the planet</returns>
        private string GetPlanetName(int planetIndex, string subtypeId)
        {
            string namingScheme = MySettings.Static.Settings.PlanetNameFormat;

            string name = namingScheme.SetProperty("ObjectNumber", planetIndex + 1)
                                      .SetProperty("ObjectNumberGreek", MyNamingUtils.GREEK_LETTERS[planetIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", MyNamingUtils.ConvertNumberToRoman(planetIndex + 1))
                                      .SetProperty("ObjectLetterLower", (char)('a' + (planetIndex % 26)))
                                      .SetProperty("ObjectLetterUpper", (char)('A' + (planetIndex % 26)))
                                      .SetProperty("ObjectId", subtypeId);

            return name;
        }

        /// <summary>
        /// Generates a name for the moon based on the naming scheme
        /// defined in the global settings file
        /// </summary>
        /// <param name="moonIndex">Index of the moon</param>
        /// <param name="subtypeId">Type of moon</param>
        /// <param name="parentPlanetName">Name of the parent planet</param>
        /// <returns>Name of the moon</returns>
        private string GetMoonName(int moonIndex, string subtypeId, string parentPlanetName)
        {
            string namingScheme = MySettings.Static.Settings.MoonNameFormat;

            string name = namingScheme.SetProperty("ObjectNumber", moonIndex + 1)
                                      .SetProperty("ObjectNumberGreek", MyNamingUtils.GREEK_LETTERS[moonIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", MyNamingUtils.ConvertNumberToRoman(moonIndex + 1))
                                      .SetProperty("ObjectLetterLower", (char)('a' + (moonIndex % 26)))
                                      .SetProperty("ObjectLetterUpper", (char)('A' + (moonIndex % 26)))
                                      .SetProperty("ObjectId", subtypeId)
                                      .SetProperty("MoonPlanetName", parentPlanetName);

            return name;
        }

        /// <summary>
        /// Returns the storage name for the given planet
        /// </summary>
        /// <param name="planet">Planet to get a storage name for</param>
        /// <returns>The storage name of the planet</returns>
        public static string GetPlanetStorageName(MySystemPlanet planet)
        {
            return (planet.DisplayName).Replace(" ", "_");
        }

        /// <summary>
        /// Gets the object name from a storage name retreived with GetPlanetStorageName
        /// </summary>
        /// <param name="storageName">The storage name</param>
        /// <returns>The display name of the planet</returns>
        public static string GetPlanetNameForPlanetStorageName(string storageName)
        {
            return storageName.Replace("_", " ");
        }
    }
}
