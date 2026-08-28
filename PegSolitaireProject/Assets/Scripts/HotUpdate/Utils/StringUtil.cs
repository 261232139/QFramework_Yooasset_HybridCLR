using System;

namespace HotUpdate.Utils
{
    public static class StringUtil
    {
        /// <summary>
        /// Converts bytes to an uppercase hexadecimal string.
        /// </summary>
        public static string ByteArrayToHexString(byte[] bytes, string separator = " ")
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return BitConverter.ToString(bytes).Replace("-", separator ?? string.Empty);
        }
    }
}
