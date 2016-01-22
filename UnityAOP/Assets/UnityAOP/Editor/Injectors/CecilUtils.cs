using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Assets.UnityAOP.Editor.Injectors {
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

        public static FieldDefinition FindField(this TypeDefinition typeDef, String fieldName) {
            return typeDef.Fields.FirstOrDefault(x => x.Name == fieldName);
        }

        public static MethodDefinition FindMethod(this TypeDefinition typeDef, String methodName, List<TypeReference> parameters, bool isStatic) {
            foreach (var method in typeDef.Methods) {
                if (method.IsStatic != isStatic) continue;
                if (method.Parameters.Count != parameters.Count) continue;
                if (method.Name != methodName) continue;

                bool mismatchParams = false;
                for (int i = 0; i < method.Parameters.Count; ++i) {
                    if (method.Parameters[i].ParameterType != parameters[i]) {
                        mismatchParams = true;
                        break;
                    }
                }
                if (mismatchParams) continue;

                return method;
            }

            return null;
        }
    
        public static void AddAttribute<T>(this AssemblyDefinition assemblyDef) {
            var module = assemblyDef.MainModule;
            var type = typeof (T);
            var attributeDef = module.FindTypeDefinition(type);

            MethodReference attributeConstructor = module.ImportReference(attributeDef.GetConstructors().First());
            CustomAttribute attribute = new CustomAttribute(attributeConstructor);
            assemblyDef.CustomAttributes.Add(attribute);
        }
    
        public static TypeDefinition FindTypeDefinition<T>(this ModuleDefinition module) {
            Type t = typeof (T);
            return module.FindTypeDefinition(t);
        }
    
        public static TypeDefinition FindTypeDefinition(this ModuleDefinition module, Type type) {
            return module.Types.FirstOrDefault(x => x.Name == type.Name);
        }
    
        public static TypeDefinition FindTypeDefinition(this ModuleDefinition module, String typeName) {
            return module.Types.FirstOrDefault(x => x.Name == typeName);
        }
    
        public static MethodDefinition FindMethodDefinition(this TypeDefinition type, String method) {
            return type.Methods.FirstOrDefault(x => x.Name == method);
        }
    
        public static TypeDefinition AddInterface<T>(this TypeDefinition type) {
            var module = type.Module;
            var interfaceDef = module.FindTypeDefinition<T>();
            var interfaceRef = module.ImportReference(interfaceDef);
            type.Interfaces.Add(interfaceRef);
            return interfaceDef;
        }
    
        public static MethodDefinition OverrideMethod(this TypeDefinition targetTypeDef, String baseMethodName) {
            var module = targetTypeDef.Module;
            var baseTypeDef = module.Types.FirstOrDefault(x => x.FullName == targetTypeDef.BaseType.FullName);
            var baseMethodDef = baseTypeDef.Methods.FirstOrDefault(x => x.Name == baseMethodName);
    
            var newMethodAttributes = baseMethodDef.Attributes & ~MethodAttributes.NewSlot | MethodAttributes.ReuseSlot;
            var newMethodDef = new MethodDefinition(baseMethodDef.Name, newMethodAttributes, baseMethodDef.ReturnType);
    
            newMethodDef.ImplAttributes = baseMethodDef.ImplAttributes;
            newMethodDef.SemanticsAttributes = baseMethodDef.SemanticsAttributes;
    
            foreach (var arg in baseMethodDef.Parameters) {
                newMethodDef.Parameters.Add(new ParameterDefinition(arg.Name, arg.Attributes, arg.ParameterType));
            }
    
            targetTypeDef.Methods.Add(newMethodDef);
            return newMethodDef;
        }
    
        public static MethodDefinition AddInterfaceMethod(this TypeDefinition targetTypeDef, TypeDefinition interfaceTypeDef, String interfaceMethodName) {
            MethodDefinition interfaceMethodDef = interfaceTypeDef.Methods.First(x => x.Name == interfaceMethodName);
    
            MethodAttributes newMethodAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig;
            MethodDefinition newMethodDef = new MethodDefinition(interfaceMethodDef.Name, newMethodAttributes, interfaceMethodDef.ReturnType);
    
            newMethodDef.ImplAttributes = interfaceMethodDef.ImplAttributes;
            newMethodDef.SemanticsAttributes = interfaceMethodDef.SemanticsAttributes;
            
            foreach (var arg in interfaceMethodDef.Parameters) {
                newMethodDef.Parameters.Add(new ParameterDefinition(arg.Name, arg.Attributes, arg.ParameterType));
            }
    
            targetTypeDef.Methods.Add(newMethodDef);
            return newMethodDef;
        }
    
        public static void InsertBefore(this ILProcessor ilProc, Instruction target, Instruction[] instructions) {
            foreach (var instruction in instructions) {
                ilProc.InsertBefore(target, instruction);
            }
        }
    
        public static bool HasInterface<T>(this TypeDefinition typeDef) {
            var typeName = typeof (T).Name;
            return typeDef.Interfaces.Any(x => x.Name == typeName);
        }
    
        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] arguments) {
            var reference = new MethodReference(self.Name, self.ReturnType, self.DeclaringType.MakeGenericInstanceType(arguments)) {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };
    
            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
    
            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
    
            return reference;
        }

        public static IEnumerable<TypeDefinition> Parents(this TypeDefinition self) {
            TypeDefinition cur = self;
            while (cur.BaseType != null) {
                cur = cur.BaseType.Resolve();
                yield return cur;
            }
        }
    }
}
