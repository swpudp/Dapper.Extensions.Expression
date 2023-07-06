using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Utilities
{
    internal static class NamingUtils
    {
        internal static Dictionary<NamingPolicy, Func<string, string>> NamingPolicyHandlers = new Dictionary<NamingPolicy, Func<string, string>>
        {
            [NamingPolicy.None] = s => s,
            [NamingPolicy.CamelCase] = NamingUtils.GetCamelCaseName,
            [NamingPolicy.LowerCase] = NamingUtils.GetLowerCaseName,
            [NamingPolicy.SnakeCase] = NamingUtils.GetSnakeCaseName,
            [NamingPolicy.UpperCase] = NamingUtils.GetUpperCaseName,
            [NamingPolicy.UpperSnakeCase] = NamingUtils.GetUpperSnakeCaseName
        };

        internal static string GetCamelCaseName(string name)
        {
            return string.IsNullOrEmpty(name) ? name : char.ToLower(name[0]) + name.Substring(1);
        }

        internal static string GetLowerCaseName(string name)
        {
            return name.ToLower();
        }

        internal static string GetSnakeCaseName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var builder = new StringBuilder(name.Length + Math.Min(2, name.Length / 5));
            var previousCategory = default(UnicodeCategory?);

            for (var currentIndex = 0; currentIndex < name.Length; currentIndex++)
            {
                var currentChar = name[currentIndex];
                if (currentChar == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }

                var currentCategory = char.GetUnicodeCategory(currentChar);
                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            previousCategory != null &&
                            currentIndex > 0 &&
                            currentIndex + 1 < name.Length &&
                            char.IsLower(name[currentIndex + 1]))
                        {
                            builder.Append('_');
                        }

                        currentChar = char.ToLower(currentChar);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }
                builder.Append(currentChar);
                previousCategory = currentCategory;
            }
            return builder.ToString();
        }

        internal static string GetUpperCaseName(string name) { return name.ToUpper(); }

        internal static string GetUpperSnakeCaseName(string name)
        {
            return GetSnakeCaseName(name).ToUpper();
        }
    }
}
