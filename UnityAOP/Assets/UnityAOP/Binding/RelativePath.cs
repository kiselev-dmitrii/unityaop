using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
    public class RelativePath : Node {
        [HideInInspector]
        public String Path;

    }
}