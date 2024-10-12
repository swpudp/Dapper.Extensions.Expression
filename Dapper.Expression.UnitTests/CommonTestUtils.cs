using System;
using System.Security.Cryptography;
using System.Text;

namespace Dapper.Extensions.Expression.UnitTests
{
    internal static class CommonTestUtils
    {

        private static Random _random = new Random();

        private static string[] extensions = { ".pdf", ".doc", ".gif", ".jpg", ".xls" };

        public static string GetRandomString(int length)
        {
            byte[] bytes = new byte[length];
            Span<byte> span = new Span<byte>(bytes, 0, bytes.Length);
            RandomNumberGenerator.Fill(span);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in span)
            {
                builder.AppendFormat("{0:X2}", b);
            }
            return builder.ToString();
        }

        public static string GetRandomExt()
        {
            return extensions[_random.Next(extensions.Length)];
        }

        public static int GetRandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
