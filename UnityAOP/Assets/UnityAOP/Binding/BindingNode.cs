using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
    public class BindingNode : MonoBehaviour {
        [ClassHasAttribute(typeof(ObservableAttribute))]
        public SerializableType Type;

        [HideInInspector]
        public String Path;

    }
}