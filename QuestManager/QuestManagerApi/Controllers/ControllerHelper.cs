namespace QuestManagerClientApi.Controllers
{
    internal static class ControllerHelper
    {

        public static string RetreiveDelimitedData(string delimiter, string readFile)
        {
            if (string.IsNullOrEmpty(readFile))
                return string.Empty;

            int startIndex = readFile.IndexOf(delimiter) + delimiter.Length;
            int endIndex = readFile.IndexOf(delimiter, startIndex);

            if (startIndex != -1 && endIndex != -1)
                return readFile.Substring(startIndex, endIndex - startIndex).Trim();

            return string.Empty;
        }
    }
}