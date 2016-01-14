using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Utils {
public static class CollectionUtils {
    public static int IndexOf<T>(this IEnumerable<T> collection, Predicate<T> predicate) {
        int result = 0;
        foreach (var item in collection) {
            if (predicate(item)) return result;
            ++result;
        }
        return -1;
    }
}
}
