using Discord;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AetherLink.Utility
{
    public static class EnumHelper
    {
        public static List<string> GetEnumChoices<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => e.ToString())
                .Where(name => name.Length < 25) 
                .ToList();
        }

        public static bool IsValidEnumMember<T>(string input) where T : struct, Enum
        {
            return Enum.TryParse<T>(input, true, out _);
        }
        public static bool TryConvertToEnum<T>(string input, out T result) where T : struct, Enum
        {
            return Enum.TryParse(input, true, out result);
        }
    }
}
