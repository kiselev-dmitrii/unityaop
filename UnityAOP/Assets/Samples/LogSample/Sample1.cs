using System;
using Assets.UnityAOP.Aspect.BoundaryAspect;
using UnityEngine;

namespace Assets.Samples.LogSample {
public class Sample1 : MonoBehaviour {
    private Warrior warrior;

    public void Awake() {
        warrior = new Warrior();
    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            warrior.Say("Hello, world");
        }
    }
}

public class Warrior {
    public void Attack() {
        UnityEngine.Debug.Log("Warrior attacks");
    }

    [Log]
    public void MakeScream() {
        UnityEngine.Debug.Log("Scream!!");
    }

    [Log]
    public void Say(String text) {
        UnityEngine.Debug.Log(text);
    }
}
}
