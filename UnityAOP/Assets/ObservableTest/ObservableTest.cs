using System;
using Assets.UnityAOP.Observable;
using UnityEngine;

namespace Assets.ObservableTest {
public class ObservableTest : MonoBehaviour {
    private Player player;

    public void Awake() {
        player = new Player();
    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            foreach (var type in ObservableMetadata.GetAllTypesMetadata()) {
                Debug.Log(type);
            }
        }
    }
}
}
