using System;
using Assets.UnityAOP.Observable;
using UnityEngine;
using Application = Assets.ObservableTest.Model.Application;

namespace Assets.ObservableTest {
    public interface Interface {
        void IncreaseValue();
    }

    public class Implementation : Interface {
        public int value;

        public void IncreaseValue() {
            value++;
        }
    }

public class ObservableTest : MonoBehaviour {
    public Application Application;

    public Implementation ReferenceProperty { get; private set; }
    public object getterDelegate;

    public Func<Interface> getterDelegateTyped;
    
    public void Awake() {
        Application = new Application();

        getterDelegate = new Func<Interface>(delegate { return ReferenceProperty; });

        getterDelegateTyped = (Func<Interface>)getterDelegate;
    }

    public void OnGUI() {
        ReferenceProperty = new Implementation();
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            Profiler.BeginSample("Get by delegate");
            for (int i = 0; i < 1000000; ++i) {
                ReferenceProperty.IncreaseValue();
            }
        }
    }
}
}
