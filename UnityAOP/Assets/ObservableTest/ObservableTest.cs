using System;
using Assets.UnityAOP.Observable;
using UnityEngine;
using Application = Assets.ObservableTest.Model.Application;

namespace Assets.ObservableTest {
public class ObservableTest : MonoBehaviour {
    public Application Application;

    public int ValueProperty { get; private set; }
    public object getterDelegate;

    public GetterDelegate<int> getterDelegateTyped;
    
    public void Awake() {
        Application = new Application();

        getterDelegate = new GetterDelegate<int>(delegate { return ValueProperty; });

        getterDelegateTyped = (GetterDelegate<int>) getterDelegate;
    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            Profiler.BeginSample("Get by delegate");
            for (int i = 0; i < 1000000; ++i) {
                ValueProperty = ValueProperty + 1;
            }
        }
    }
}
}
