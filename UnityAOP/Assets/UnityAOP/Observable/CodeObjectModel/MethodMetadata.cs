using System;
using System.Linq;
using System.Reflection;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public class MethodMetadata : MemberMetadata {
        public Type[] Parameters { get; private set; }

        public MethodMetadata(MethodInfo methodInfo) : base(methodInfo.Name, methodInfo.ReturnType, MemberType.Method) {
            Parameters = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
        }
    }
}