using System;

namespace Assets.UnityAOP.Aspect.PartialAdvice {
    public class AfterMethodAttribute : Attribute {
        public String MethodName { get; private set; }

        public AfterMethodAttribute(String methodName) {
            MethodName = methodName;
        }
    }
}