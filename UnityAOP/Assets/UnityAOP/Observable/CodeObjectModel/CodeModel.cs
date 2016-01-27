using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.UnityAOP.Observable.Core;
using Assets.UnityAOP.Utils;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public static class CodeModel {
        private static Dictionary<String, TypeMetadata> stringToTypemeta;
        private static Dictionary<Type, TypeMetadata> typeToTypemeta;
        private static bool isInited = false;

        public static void Initialize() {
            if (isInited) return;

            stringToTypemeta = new Dictionary<string, TypeMetadata>();
            typeToTypemeta = new Dictionary<Type, TypeMetadata>();
    
            var assembly = Assembly.GetExecutingAssembly();
            var observableTypes = assembly.GetTypes().Where(x => x.HasAttribute<ObservableAttribute>());

            foreach (var observableType in observableTypes) {
                var meta = new TypeMetadata(observableType);
                stringToTypemeta.Add(meta.Name, meta); 
                typeToTypemeta.Add(meta.Type, meta);
            }

            isInited = true;
        }

        public static TypeMetadata GetType(String typeName) {
            return stringToTypemeta[typeName];
        }
    
        public static TypeMetadata GetType(Type type) {
            return typeToTypemeta.Get(type);
        }
    
        public static IEnumerable<TypeMetadata> GetTypes() {
            return stringToTypemeta.Values;
        } 
    }
}
