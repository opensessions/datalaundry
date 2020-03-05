using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLaundryDAL.Utilities
{
    public static class EnumerableExtensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }


        public static bool IsNull<T>(this T thing)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            return thing == null;
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        public static bool IsNotNull<T>(this T thing)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            return thing != null;
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        //use for remove duplicate row sepecefic column
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            if (source != null)
            {
                foreach (TSource element in source)
                {
                    if (seenKeys.Add(keySelector(element)))
                    {
                        yield return element;
                    }
                }
            }
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz#$%*()_+!@~ABCDEFGHIJ0987654321KLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}