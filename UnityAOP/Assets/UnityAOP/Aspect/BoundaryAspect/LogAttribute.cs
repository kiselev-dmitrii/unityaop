using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Assets.UnityAOP.Aspect.BoundaryAspect {
public class LogAttribute : BaseBoundaryAttribute {
    public override void OnEnter(MethodBase method, Dictionary<string, object> parameters) {
        StringBuilder text = new StringBuilder();
        text.Append("Calling method ");
        text.Append(method.Name);

        foreach (var param in parameters) {
            text.Append("\n" + param.Key + "=" + param.Value);
        }

        Debug.Log(text.ToString());
    }

    public override void OnExit() {
        Debug.Log("Exit");
    }
}
}
