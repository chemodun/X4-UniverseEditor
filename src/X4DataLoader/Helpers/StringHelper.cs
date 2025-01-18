namespace X4DataLoader.Helpers
{
    public static class StringHelper
    {
        public static bool EqualsIgnoreCase(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
