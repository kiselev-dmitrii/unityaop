using Assets.UnityAOP.Observable;
using UnityEngine;

namespace Assets.ObservableTest {
public class ObservableTest : MonoBehaviour {
    private Player player;

    public void Awake() {
        player = new Player();
    }

    public void OnGUI() {
        object playerObject = (object) player;
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            if (playerObject is IObservable) {
                Debug.Log("Is Observable");
            } else {
                Debug.Log("Is not Observable");
            }
        }
    }
}
}
