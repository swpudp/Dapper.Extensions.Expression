using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Utilities
{
    public static class NamingUtils
    {
        private static readonly Dictionary<NamingPolicy, Func<string, string>> NamingPolicyHandlers = new Dictionary<NamingPolicy, Func<string, string>>
        {
            [NamingPolicy.CamelCase] = GetCamelCaseName,
            [NamingPolicy.LowerCase] = GetLowerCaseName,
            [NamingPolicy.SnakeCase] = GetSnakeCaseName,
            [NamingPolicy.UpperCase] = GetUpperCaseName,
            [NamingPolicy.UpperSnakeCase] = GetUpperSnakeCaseName
        };

        private static string GetCamelCaseName(string name)
        {
            return string.IsNullOrEmpty(name) ? name : char.ToLower(name[0]) + name.Substring(1);
        }

        private static string GetLowerCaseName(string name)
        {
            return name.ToLower();
        }

        private static string GetSnakeCaseName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            StringBuilder builder = new StringBuilder(name.Length + Math.Min(2, name.Length / 5));
            UnicodeCategory? previousCategory = default;

            for (int currentIndex = 0; currentIndex < name.Length; currentIndex++)
            {
                char currentChar = name[currentIndex];
                if (currentChar == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }

                UnicodeCategory currentCategory = char.GetUnicodeCategory(currentChar);
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

        private static string GetUpperCaseName(string name) { return name.ToUpper(); }

        private static string GetUpperSnakeCaseName(string name)
        {
            return GetSnakeCaseName(name).ToUpper();
        }

        private static NamingPolicy? currentNamingPolicy;

        public static void SetNamingPolicy(NamingPolicy namingPolicy)
        {
            currentNamingPolicy = namingPolicy;
        }

        internal static string GetName(string name)
        {
            return currentNamingPolicy == null ? name : NamingPolicyHandlers[currentNamingPolicy.Value](name);
        }

        [Obsolete("使用全局设置", true)]
        internal static string GetName(NamingPolicy namingPolicy, string name)
        {
            return NamingPolicyHandlers[namingPolicy](name);
        }

        [Obsolete("使用全局设置", true)]
        internal static FieldNamingAttribute GetNamingAttribute(MemberInfo member)
        {
            FieldNamingAttribute namingAttribute = member.ReflectedType.GetCustomAttribute<FieldNamingAttribute>(true);
            if (namingAttribute != null)
            {
                return namingAttribute;
            }
            return member.DeclaringType.GetCustomAttribute<FieldNamingAttribute>(true);
        }
    }
}
