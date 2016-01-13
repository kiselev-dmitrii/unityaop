using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

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

    public static CustomAttribute FindAtributeInheritedFrom<T>(this MethodDefinition method) {
        Type t = typeof(T);
        return method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Resolve().BaseType.Name == t.Name);
    }

    public static void AddAttribute<T>(this AssemblyDefinition assemblyDef) {
        var module = assemblyDef.MainModule;
        
        MethodReference attributeConstructor = module.Import(typeof(T).GetConstructor(Type.EmptyTypes));
        CustomAttribute attribute = new CustomAttribute(attributeConstructor);
        assemblyDef.CustomAttributes.Add(attribute);
    }

    public static TypeDefinition FindTypeDefinition<T>(this ModuleDefinition module) {
        Type t = typeof (T);
        return module.Types.FirstOrDefault(x => x.Name == t.Name);
    }

    public static MethodDefinition FindMethodDefinition(this TypeDefinition type, String method) {
        return type.Methods.FirstOrDefault(x => x.Name == method);
    }

}
}
