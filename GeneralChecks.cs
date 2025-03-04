namespace Tools
{
    public static class GeneralChecks
    {
        /// <summary>
        /// Check if the file name contains a directory
        /// </summary>
        /// <param name="fileName">File name that need to be checked</param>
        /// <returns>True if the file name contains invalid symbols</returns>
        public static bool CheckFileName(string fileName)
        {
            if (fileName.Contains('\\') || fileName.Contains('/') || fileName.Contains(':') || fileName.Contains('*') || fileName.Contains('?') || fileName.Contains('\'') || fileName.Contains('<') || fileName.Contains('>') || fileName.Contains('|'))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method used to remove all spaces from a string
        /// </summary>
        /// <param name="input">La stringa da filtrare.</param>
        /// <returns>La stringa filtrata.</returns>
        public static string RemoveSpaces(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
