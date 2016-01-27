using UnityEditor;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    [InitializeOnLoad]
    public static class OnApplicationStarted {
        static OnApplicationStarted() {
            CodeModel.Initialize();
        }
    }
}
