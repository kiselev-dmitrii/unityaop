using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return type.HasAttribute(typeof (T));
        }

        public static bool HasAttribute(this Type type, Type attributeType) {
            return type.GetCustomAttributes(attributeType, true).Length > 0;
        }
    
        public static bool ImplementInterface<T>(this Type type) {
            Type t = typeof (T);
            return type.GetInterfaces().Contains(t);
        }
    }
}
