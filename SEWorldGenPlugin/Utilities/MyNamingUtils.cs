namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Utility for naming conventions
    /// </summary>
    public class MyNamingUtils
    {
        /// <summary>
        /// Greek letters used with the name formats.
        /// </summary>
        public static readonly string[] GREEK_LETTERS = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "My", "Ny", "Xi", "Omikron", "Pi", "Rho", "Sigma", "Tau", "Ypsilon", "Phi", "Chi", "Psi", "Omega" };

        /// <summary>
        /// Converts an integer to roman numerals
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>The converted number in roman numerals.</returns>
        public static string ConvertNumberToRoman(int number)
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
    }
}
