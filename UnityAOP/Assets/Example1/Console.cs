using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Example1 {
public class Console : MonoBehaviour {
    public static Console Instance { get; private set; }
    private StringBuilder content;

    protected void Awake() {
        Instance = this;
        content = new StringBuilder();
    }

    public void Add(String message) {
        content.Append(message + '\n');
    }


    protected void OnGUI() {
        GUI.TextArea(new Rect(10, 10, 800, 600), content.ToString());
    }
}
}
