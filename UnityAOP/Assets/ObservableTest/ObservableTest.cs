using System;
using UnityEngine;
using Application = Assets.ObservableTest.Model.Application;

namespace Assets.ObservableTest {
public class ObservableTest : MonoBehaviour {
    public Application Application;

    public void Awake() {
        Application = new Application();
    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            Application.Player.Group.NumMembers++;
        }
    }
}
}
