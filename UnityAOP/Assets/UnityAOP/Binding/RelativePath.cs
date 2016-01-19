using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
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