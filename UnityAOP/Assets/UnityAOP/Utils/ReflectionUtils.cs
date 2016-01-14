﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Assets.UnityAOP.Utils {
public static class ReflectionUtils {
    public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly) {
        foreach (Type type in assembly.GetTypes()) {
            if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                yield return type;
            }
        }
    }

    public static bool HasAttribute<T>(this Type type) {
        return type.GetCustomAttributes(typeof (T), true).Length > 0;
    }
}
}