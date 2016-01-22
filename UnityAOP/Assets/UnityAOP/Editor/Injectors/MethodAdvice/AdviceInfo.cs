using System;
using System.Collections.Generic;
using Assets.UnityAOP.Aspect.MethodAdvice;
using Mono.Cecil;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Editor.Injectors.MethodAdvice {
    public class AdviceInfo {
        public TypeDefinition TypeDef;
        public MethodDefinition MethodDef;

        public TypeDefinition TargetType;
        public String TargetMethodName;
        public List<TypeReference> TargetParameters;
        public bool IsStatic;
        public bool IsConstructor;

        public MethodAdvicePhase Phase;

        public AdviceInfo(TypeDefinition type, MethodDefinition method, CustomAttribute attribute) {
            TypeDef = type;
            MethodDef = method;

            TargetType = (TypeDefinition)attribute.ConstructorArguments[0].Value;
            TargetMethodName = (String)attribute.ConstructorArguments[1].Value;
            Phase = (MethodAdvicePhase)attribute.ConstructorArguments[2].Value;
            IsConstructor = TargetMethodName == "Constructor";

            var parameters = MethodDef.Parameters;
            if (parameters.Count == 0) {
                IsStatic = true;
                return;
            }

            ParameterDefinition firstParam = parameters[0];
            ParameterDefinition lastParam = parameters[parameters.Count - 1];
            IsStatic = firstParam.ParameterType.Resolve() != TargetType;
            
            TargetParameters = new List<TypeReference>();
            int skipFirst = IsStatic ? 0 : 1;
            int skipLast = lastParam.Name == "returnValue" ? 1 : 0;
            for (int i = skipFirst; i < parameters.Count - skipLast; ++i) {
                TargetParameters.Add(parameters[i].ParameterType);
            }
        }

        public String Check() {
            if (!MethodDef.IsStatic) {
                return "Advice method must be static";
            }

            if (IsConstructor && IsStatic) {
                if (TargetParameters.Count == 0) {
                    return "Static constructor cannot have parameters";
                }
            }
            return null;
        }

        public override String ToString() {
            return String.Format("{0}::{1}", TypeDef, MethodDef);
        }
    }
}