using System;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding {
    public class RelativePath : Node {
        [HideInInspector]
        public String Path;
        private String fullPath;

        public override string GetFullPath() {
            if (fullPath == null) {
                if (ParentNode == GetRootNode()) {
                    return Path;
                } else {
                    fullPath = ParentNode.GetFullPath() + "." + Path;
                }
            }
            return fullPath;
        }
    }
}