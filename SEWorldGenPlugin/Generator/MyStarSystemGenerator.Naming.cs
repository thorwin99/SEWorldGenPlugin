using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.Generator
{
    public partial class MyStarSystemGenerator
    {
        public static readonly string PROP_OBJ_NUMBER = "ObjectNumber";
        public static readonly string PROP_OBJ_NUMBER_GREEK = "ObjectNumberGreek";
        public static readonly string PROP_OBJ_NUMBER_ROMAN = "ObjectNumberRoman";
        public static readonly string PROP_OBJ_LETTER_LOWER = "ObjectLetterLower";
        public static readonly string PROP_OBJ_LETTER_UPPER = "ObjectLetterUpper";
        public static readonly string PROP_OBJ_ID = "ObjectId";
        public static readonly string PROP_OBJ_PARENT = "ParentName";


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

            string name = namingScheme.SetProperty(PROP_OBJ_NUMBER, planetIndex + 1)
                                      .SetProperty(PROP_OBJ_NUMBER_GREEK, MyNamingUtils.GREEK_LETTERS[planetIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty(PROP_OBJ_NUMBER_ROMAN, MyNamingUtils.ConvertNumberToRoman(planetIndex + 1))
                                      .SetProperty(PROP_OBJ_LETTER_LOWER, (char)('a' + (planetIndex % 26)))
                                      .SetProperty(PROP_OBJ_LETTER_UPPER, (char)('A' + (planetIndex % 26)))
                                      .SetProperty(PROP_OBJ_ID, subtypeId);

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

            string name = namingScheme.SetProperty(PROP_OBJ_NUMBER, moonIndex + 1)
                                      .SetProperty(PROP_OBJ_NUMBER_GREEK, MyNamingUtils.GREEK_LETTERS[moonIndex % MyNamingUtils.GREEK_LETTERS.Length])
                                      .SetProperty(PROP_OBJ_NUMBER_ROMAN, MyNamingUtils.ConvertNumberToRoman(moonIndex + 1))
                                      .SetProperty(PROP_OBJ_LETTER_LOWER, (char)('a' + (moonIndex % 26)))
                                      .SetProperty(PROP_OBJ_LETTER_UPPER, (char)('A' + (moonIndex % 26)))
                                      .SetProperty(PROP_OBJ_ID, subtypeId)
                                      .SetProperty(PROP_OBJ_PARENT, parentPlanetName);

            return name;
        }

        /// <summary>
        /// Gets the object name from a storage name retreived with GetPlanetStorageName
        /// </summary>
        /// <param name="storageName">The storage name</param>
        /// <returns>The display name of the planet</returns>
        public static string GetPlanetNameForPlanetStorageName(string storageName)
        {
            return storageName.Substring(0, storageName.LastIndexOf("-")).Replace("_", " ");
        }
    }
}
