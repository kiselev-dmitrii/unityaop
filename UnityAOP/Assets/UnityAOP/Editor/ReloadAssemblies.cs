using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Assets.UnityAOP.Editor {
public class ReloadAssemblies {
    [MenuItem("Assets/ReloadAssemblies")]
    public static void Reload() {
        UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
    }
}
}
