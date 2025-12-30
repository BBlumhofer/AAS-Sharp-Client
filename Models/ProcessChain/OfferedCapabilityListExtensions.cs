using System;
using System.Collections.Generic;
using System.Linq;

namespace AasSharpClient.Models.ProcessChain
{
    /// <summary>
    /// Extension helpers to manipulate lists of <see cref="OfferedCapability"/> in a reusable way.
    /// </summary>
    public static class OfferedCapabilityListExtensions
    {
        public static void InsertRangeBefore(this IList<OfferedCapability> list, IEnumerable<OfferedCapability> items)
        {
            if (list == null || items == null) return;
            var toInsert = items.Where(i => i != null).ToList();
            if (toInsert.Count == 0) return;
            list.InsertRange(0, toInsert);
        }

        public static void InsertRangeAfter(this IList<OfferedCapability> list, IEnumerable<OfferedCapability> items)
        {
            if (list == null || items == null) return;
            foreach (var it in items)
            {
                if (it != null) list.Add(it);
            }
        }

        public static void InsertRangeAt(this IList<OfferedCapability> list, int index, IEnumerable<OfferedCapability> items)
        {
            if (list == null || items == null) return;
            var toInsert = items.Where(i => i != null).ToList();
            if (toInsert.Count == 0) return;
            if (index <= 0)
            {
                list.InsertRange(0, toInsert);
                return;
            }
            if (index >= list.Count)
            {
                foreach (var it in toInsert) list.Add(it);
                return;
            }
            list.InsertRange(index, toInsert);
        }

        // Helper to provide InsertRange on IList<T> when underlying implementation is List<T>
        private static void InsertRange<T>(this IList<T> list, int index, IEnumerable<T> items)
        {
            if (list is List<T> concrete)
            {
                concrete.InsertRange(index, items);
                return;
            }

            var toInsert = items.ToList();
            if (index <= 0)
            {
                var newList = toInsert.Concat(list).ToList();
                list.Clear();
                foreach (var it in newList) list.Add(it);
                return;
            }

            var before = list.Take(index).ToList();
            var after = list.Skip(index).ToList();
            list.Clear();
            foreach (var it in before) list.Add(it);
            foreach (var it in toInsert) list.Add(it);
            foreach (var it in after) list.Add(it);
        }
    }
}
