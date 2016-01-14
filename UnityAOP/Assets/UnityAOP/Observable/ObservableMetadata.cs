using System;
using System.Collections.Generic;

namespace Assets.UnityAOP.Observable {
public class ClassMetadata {
    public String Name;
    public Dictionary<String, PropertyMetadata> Properties;

    public ClassMetadata(String name, PropertyMetadata[] properties) {
        Name = name;
        foreach (var property in properties) {
            Properties[property.Name] = property;
        }
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
}

public static class ObservableMetadata {
    private static readonly Dictionary<String, ClassMetadata> Classes;

    static ObservableMetadata() {
        Classes = new Dictionary<string, ClassMetadata>();

        AddClassMetadata(new ClassMetadata("BaseObservable", new[] {
            new PropertyMetadata("NumFriends", 0, typeof(int)),
            new PropertyMetadata("Name", 1, typeof(String)), 
        }));
    }

    public static ClassMetadata GetClassMetadata(String className) {
        return Classes[className];
    }

    private static void AddClassMetadata(ClassMetadata metadata) {
        Classes.Add(metadata.Name, metadata);
    }
}
}
