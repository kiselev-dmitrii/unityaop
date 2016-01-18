using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;
using UnityEditor;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable {
    public class TypeMetadata {
        public String Name;
        public Type Type;
        public Dictionary<String, PropertyMetadata> Properties;
    
        public TypeMetadata(Type type, Dictionary<String, PropertyMetadata> properties) {
            Name = type.Name;
            Type = type;
            Properties = properties;
        }
    
        public PropertyMetadata GetPropertyMetadata(String name) {
            return Properties.Get(name);
        }
    
        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name + '\n');
            foreach (var property in Properties.Values) {
                builder.Append(property.ToString() + '\n');
            }
            return builder.ToString();
        }
    }
    
    public class PropertyMetadata {
        public String Name;
        public Int32 Index;
        public Type Type;
    
        public bool IsCollection;
        public Type ItemType;
    
        public PropertyMetadata(String name, int index, Type type) {
            Name = name;
            Index = index;
            Type = type;
            IsCollection = type.GetInterface("IObservableCollection`1") != null;
            if (IsCollection) {
                ItemType = type.GetGenericArguments()[0];
            }
        }
    
        public override string ToString() {
            return String.Format("{0} {1} {2}", Index, Type.Name, Name);
        }
    }
    
    [InitializeOnLoad]
    public static class ObservableMetadata {
        private static readonly Dictionary<String, TypeMetadata> stringToTypemeta;
        private static readonly Dictionary<Type, TypeMetadata> typeToTypemeta; 
    
        static ObservableMetadata() {
            stringToTypemeta = new Dictionary<string, TypeMetadata>();
            typeToTypemeta = new Dictionary<Type, TypeMetadata>();
    
            var assembly = Assembly.GetExecutingAssembly();
            var observableTypes = assembly.GetTypes().Where(x => x.HasAttribute<ObservableAttribute>());
            foreach (var observableType in observableTypes) {
                var meta = CreateTypeMetadata(observableType);
                stringToTypemeta.Add(meta.Name, meta); 
                typeToTypemeta.Add(meta.Type, meta);
            }
        }
    
        public static TypeMetadata GetTypeMetadata(String typeName) {
            return stringToTypemeta[typeName];
        }
    
        public static TypeMetadata GetTypeMetadata(Type type) {
            return typeToTypemeta.Get(type);
        }
    
        public static IEnumerable<TypeMetadata> GetAllTypesMetadata() {
            return stringToTypemeta.Values;
        } 
    
        private static TypeMetadata CreateTypeMetadata(Type type) { 
            Assert.IsTrue(type.HasAttribute<ObservableAttribute>(), "Тип должен иметь аттрибут Observable ");
    
            Stack<Type> hierarchy = new Stack<Type>();
            hierarchy.Push(type);
    
            var baseType = hierarchy.Peek().BaseType;
            while (baseType.HasAttribute<ObservableAttribute>()) {
                hierarchy.Push(baseType);
                baseType = hierarchy.Peek().BaseType;
            }
    
            Dictionary<String, PropertyMetadata> propertiesMeta = new Dictionary<String, PropertyMetadata>();
            while (hierarchy.Count > 0) {
                var t = hierarchy.Pop();
    
                var properties = t.GetProperties();
                foreach (var property in properties) {
                    var propertyMeta = new PropertyMetadata(property.Name, propertiesMeta.Count, property.PropertyType);
                    if (!propertiesMeta.ContainsKey(propertyMeta.Name)) {
                        propertiesMeta.Add(propertyMeta.Name, propertyMeta);
                    }
                }
            }
    
            return new TypeMetadata(type, propertiesMeta);
        }
    }
}
