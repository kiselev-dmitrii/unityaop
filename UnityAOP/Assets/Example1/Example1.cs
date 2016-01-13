using System;
using UnityEngine;

namespace Assets.Example1 {
public class Example1 : MonoBehaviour {
    public String Good;
    public String GoodToo;
    public String Bad;

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
}
