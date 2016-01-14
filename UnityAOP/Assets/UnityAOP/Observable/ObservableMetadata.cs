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
    public Dictionary<String, PropertyMetadata> Properties;

    public TypeMetadata(String name, Dictionary<String, PropertyMetadata> properties) {
        Name = name;
        Properties = properties;
    }

    public PropertyMetadata GetPropertyMetadata(String name) {
        return Properties[name];
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

    public PropertyMetadata(String name, int index, Type type) {
        Name = name;
        Index = index;
        Type = type;
    }

    public override string ToString() {
        return String.Format("{0} {1} {2}", Index, Type.Name, Name);
    }
}

[InitializeOnLoad]
public static class ObservableMetadata {
    private static readonly Dictionary<String, TypeMetadata> types;

    static ObservableMetadata() {
        types = new Dictionary<string, TypeMetadata>();

        var assembly = Assembly.GetExecutingAssembly();
        var observableTypes = assembly.GetTypes().Where(x => x.HasAttribute<ObservableAttribute>());
        foreach (var observableType in observableTypes) {
            var meta = CreateTypeMetadata(observableType);
            types.Add(meta.Name, meta); 
        }
    }

    public static TypeMetadata GetTypeMetadata(String className) {
        return types[className];
    }

    public static IEnumerable<TypeMetadata> GetAllTypesMetadata() {
        return types.Values;
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

        return new TypeMetadata(type.Name, propertiesMeta);
    }
}
}
