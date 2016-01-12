using System;
using Assets.UnityAOP.Attributes.Attributes;
using UnityEngine;

namespace Assets.Example1 {
public class Example1 : MonoBehaviour {
    private Warrior warrior;

    public void Awake() {
        warrior = new Warrior();
    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test")) {
            warrior.Attack();
        }
    }

    [Advice(typeof(Warrior), "Attack", AdvicePhase.End)]
    public static void OnWarriorAttack() {
        Console.Instance.Add("OnWarriorAttack");
    }
}
}
