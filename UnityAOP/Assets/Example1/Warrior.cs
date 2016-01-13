using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Attributes;

namespace Assets.Example1 {
public class Warrior {
    public void Attack() {
        Console.Instance.Add("Warrior attacks");
    }

    [Log]
    public void MakeScream() {
        Console.Instance.Add("Scream!!");
    }

    [Log]
    public void Say(String text) {
        UnityEngine.Debug.Log(text);
    }
}
}
