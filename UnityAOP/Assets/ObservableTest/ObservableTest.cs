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
            if (player is IObservable) {
                IObservable observable = (IObservable) player;
                observable.AddObserver(0, null);
                observable.RemoveObserver(0, null);
                observable.GetPropertyMetadata("Name");

                Debug.Log("Is Observable");
            } else {
                Debug.Log("Is not Observable");
            }
        }
    }
}
}
