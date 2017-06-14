using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UnityEditor;


namespace Assets.UnityAOP.Editor.Injectors {
    public class TestTaskInjector {
        private readonly ModuleDefinition module;

        public TestTaskInjector(AssemblyDefinition assembly) {
            module = assembly.MainModule;
        }

        public void Inject() {
            var genericTypeDef = module.FindTypeDefinition("DerivedClass`1");
            var typeDef = CreateInstanceTypeDef(genericTypeDef, typeof (String));
            module.Types.Add(typeDef);
        }

        private TypeDefinition CreateInstanceTypeDef(TypeDefinition genericTypeDef, Type argType) {
            var argRef = module.ImportReference(argType);

            String instanceName = GetInstanceTypeName(genericTypeDef, argType);
            var instancedTypeDef = new TypeDefinition(genericTypeDef.Namespace, instanceName, genericTypeDef.Attributes, genericTypeDef.BaseType);

            foreach (var genericFieldDef in genericTypeDef.Fields) {
                var instancedField = CreateInstancedField(genericFieldDef, argRef);
                instancedTypeDef.Fields.Add(instancedField);
            }

            foreach (var genericConstructor in genericTypeDef.GetConstructors()) {
                var instancedConstructor = CreateInstancedConstructor(genericConstructor, argRef);
                instancedTypeDef.Methods.Add(instancedConstructor);
            }

            return instancedTypeDef;
        }

        private FieldDefinition CreateInstancedField(FieldDefinition genericFieldDef, TypeReference argRef) {
            TypeReference instancedFieldTypeRef = ResolveTypeReferenceRecursively(genericFieldDef.FieldType, argRef);
            var field = new FieldDefinition(genericFieldDef.Name, genericFieldDef.Attributes, instancedFieldTypeRef);
            return field;
        }

        private MethodDefinition CreateInstancedConstructor(MethodDefinition genericConstructorDef, TypeReference argRef) {
            var instancedConstructorDef = new MethodDefinition(genericConstructorDef.Name, genericConstructorDef.Attributes, genericConstructorDef.ReturnType);
            foreach (var genericParameter in genericConstructorDef.Parameters) {
                instancedConstructorDef.Parameters.Add(genericParameter);
            }
            instancedConstructorDef.Body = genericConstructorDef.Body;


            return instancedConstructorDef;
        }

        private TypeReference ResolveTypeReferenceRecursively(TypeReference typeRef, TypeReference argRef) {
            if (!typeRef.IsGenericParameter && !typeRef.ContainsGenericParameter) {
                return typeRef;
            }

            if (typeRef.IsGenericParameter) {
                return argRef;
            }

            if (typeRef is GenericInstanceType) {
                var genericInstanceType = (GenericInstanceType)typeRef;
                List<TypeReference> resolvedArguments = new List<TypeReference>();
                foreach (var genericParamter in genericInstanceType.GenericArguments) {
                    var argument = ResolveTypeReferenceRecursively(genericParamter, argRef);
                    resolvedArguments.Add(argument);
                }

                TypeReference resolvedReference = typeRef.Resolve(); //List`
                var importedRef = module.ImportReference(resolvedReference);
                return importedRef.MakeGenericInstanceType(resolvedArguments.ToArray()); //List<String>
            }

            return null;
        }

        private String GetInstanceTypeName(TypeDefinition genericTypeDef, Type genericArgType) {
            String name = genericTypeDef.Name;
            return name.Substring(0, name.IndexOf('`')) + genericArgType.Name;
        }
    }
}
