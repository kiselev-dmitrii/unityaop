using System;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
public class BindingNode : MonoBehaviour {
    [ClassImplements(typeof(IObservable))]
    public SerializableType Type;
    public String Path;
}
}