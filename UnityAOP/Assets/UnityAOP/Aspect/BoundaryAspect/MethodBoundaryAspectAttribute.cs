using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.UnityAOP.Aspect.BoundaryAspect {
[AttributeUsage(AttributeTargets.Method)]
public class MethodBoundaryAspectAttribute : Attribute {
    public virtual void OnEnter(MethodBase method, Dictionary<string, object> parameters) {
    }

    public virtual void OnExit() {
    }
}
}
