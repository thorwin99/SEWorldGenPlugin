using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.Generator
{
    public partial class MyStarSystemGenerator
    {
        /// <summary>
        /// Greek letters used with the name formats.
        /// </summary>
        private readonly string[] GREEK_LETTERS = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "My", "Ny", "Xi", "Omikron", "Pi", "Rho", "Sigma", "Tau", "Ypsilon", "Phi", "Chi", "Psi", "Omega" };

        /// <summary>
        /// Converts an integer to roman numerals
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>The converted number in roman numerals.</returns>
        private string ConvertNumberToRoman(int number)
        {
            if (number < 0) return "";
            if (number >= 1000) return "M" + ConvertNumberToRoman(number - 1000);
            if (number >= 900) return "CM" + ConvertNumberToRoman(number - 900);
            if (number >= 500) return "D" + ConvertNumberToRoman(number - 500);
            if (number >= 400) return "CD" + ConvertNumberToRoman(number - 400);
            if (number >= 100) return "C" + ConvertNumberToRoman(number - 100);
            if (number >= 90) return "XC" + ConvertNumberToRoman(number - 90);
            if (number >= 50) return "L" + ConvertNumberToRoman(number - 50);
            if (number >= 40) return "XL" + ConvertNumberToRoman(number - 40);
            if (number >= 10) return "X" + ConvertNumberToRoman(number - 10);
            if (number >= 9) return "IX" + ConvertNumberToRoman(number - 9);
            if (number >= 5) return "V" + ConvertNumberToRoman(number - 5);
            if (number >= 4) return "IV" + ConvertNumberToRoman(number - 4);
            if (number >= 1) return "I" + ConvertNumberToRoman(number - 1);
            return "";
        }

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
                                      .SetProperty("ObjectNumberGreek", GREEK_LETTERS[planetIndex % GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(planetIndex + 1))
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
            string namingScheme = MySettings.Static.Settings.PlanetNameFormat;

            string name = namingScheme.SetProperty("ObjectNumber", moonIndex + 1)
                                      .SetProperty("ObjectNumberGreek", GREEK_LETTERS[moonIndex % GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(moonIndex + 1))
                                      .SetProperty("ObjectLetterLower", (char)('a' + (moonIndex % 26)))
                                      .SetProperty("ObjectLetterUpper", (char)('A' + (moonIndex % 26)))
                                      .SetProperty("ObjectId", subtypeId)
                                      .SetProperty("MoonPlanetName", parentPlanetName);

            return name;
        }

        /// <summary>
        /// Generates a name for the asteroid belt based on the naming scheme
        /// defined in the global settings file
        /// </summary>
        /// <param name="beltIndex">Index of the belt in the system</param>
        /// <returns>Name for the belt</returns>
        private string GetBeltName(int beltIndex)
        {
            string namingScheme = MySettings.Static.Settings.BeltNameFormat;

            string name = namingScheme.SetProperty("ObjectNumber", beltIndex + 1)
                                      .SetProperty("ObjectNumberGreek", GREEK_LETTERS[beltIndex % GREEK_LETTERS.Length])
                                      .SetProperty("ObjectNumberRoman", ConvertNumberToRoman(beltIndex + 1))
                                      .SetProperty("ObjectLetterLower", (char)('a' + (beltIndex % 26)))
                                      .SetProperty("ObjectLetterUpper", (char)('A' + (beltIndex % 26)));

            return name;
        }

        /// <summary>
        /// Generates the name for an asteroid ring
        /// </summary>
        /// <param name="parentPlanetName">Name of the parent planet</param>
        /// <returns>Name for the ring</returns>
        private string GetRingName(string parentPlanetName)
        {
            return parentPlanetName + " Ring";
        }

        /// <summary>
        /// Returns the storage name for the given planet
        /// </summary>
        /// <param name="planet">Planet to get a storage name for</param>
        /// <returns>The storage name of the planet</returns>
        public static string GetPlanetStorageName(MySystemPlanet planet)
        {
            return (planet.DisplayName + "-" + planet.SubtypeId + " " + planet.CenterPosition.GetHashCode()).Replace(" ", "_");
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
