using System;

namespace Assets.UnityAOP.Aspect.PartialAdvice {
    public class BeforeMethodAttribute : Attribute {
        public String MethodName { get; private set; }

        public BeforeMethodAttribute(String methodName) {
            MethodName = methodName;
        }
    }
}
