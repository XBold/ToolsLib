using System.IO;
using static Tools.Logger.Logger;
using static Tools.Logger.Severity;

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
            if (fileName.Contains('\\') || fileName.Contains('/') || fileName.Contains(':') || fileName.Contains('*') || fileName.Contains('?') || fileName.Contains('\'') || fileName.Contains('<') || fileName.Contains('>') || fileName.Contains('|') || string.IsNullOrEmpty(fileName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the directory is valid. IT DOESN'T CHECK IF THE DIRECTORY EXISTS
        /// </summary>
        /// <param name="path">Inser the path that need to be checked</param>
        /// <returns>True if there's an error in the path</returns>
        public static bool CheckDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return true;

            try
            {
                string fullPath = Path.GetFullPath(path);

                if (!Path.IsPathRooted(fullPath))
                    return true;

                string fileName = Path.GetFileName(fullPath);

                if (string.IsNullOrWhiteSpace(fileName) || !Path.HasExtension(fileName))
                    return true;

                string? directory = Path.GetDirectoryName(fullPath);

                if (string.IsNullOrWhiteSpace(directory))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Log($"Error while checking the directory\nError: {ex.Message}", FATAL_ERROR);
                return true;
            }
        }

        /// <summary>
        /// Method used to remove all spaces from a string
        /// </summary>
        /// <param name="text">String where is ncessary to remove the spaces</param>
        /// <returns>Filtered string</returns>
        internal static string RemoveSpaces(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
