using System;

namespace Assets.UnityAOP.Attributes.Attributes {
public enum AdvicePhase {
    Begin,
    End
}

public class AdviceAttribute : Attribute {
    public Type Type { get; private set; }
    public String Method { get; private set; }
    public AdvicePhase Phase { get; private set; }

    public AdviceAttribute(Type type, String method, AdvicePhase phase) {
        Type = type;
        Method = method;
        Phase = phase;
    }
}
}
