using System;
using System.Linq;
using Mono.Cecil;

namespace Assets.UnityAOP.Editor {
public static class CecilUtils {
    public static bool HasAttributeOfType<T>(this AssemblyDefinition assemblyDefinition) {
        Type t = typeof (T);
        return assemblyDefinition.CustomAttributes.Any(x => x.AttributeType.Name == t.Name);
    }

    public static bool HasAttributeOfType<T>(this TypeDefinition typeDefinition) {
        Type t = typeof(T);
        return typeDefinition.CustomAttributes.Any(x => x.AttributeType.Name == t.Name);
    }

    public static CustomAttribute FindAttribute<T>(this MethodDefinition method) {
        Type t = typeof(T);
        return method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == t.Name);
    }

    public static void AddAttribute<T>(this AssemblyDefinition assemblyDef) {
        var module = assemblyDef.MainModule;
        
        MethodReference attributeConstructor = module.Import(typeof(T).GetConstructor(Type.EmptyTypes));
        CustomAttribute attribute = new CustomAttribute(attributeConstructor);
        assemblyDef.CustomAttributes.Add(attribute);
    }


}
}
