namespace TelegramClient.Utils
{
    public static class StringHelper
    {
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength? text : text.Substring(0, maxLength) + "...";
        }
    }
}