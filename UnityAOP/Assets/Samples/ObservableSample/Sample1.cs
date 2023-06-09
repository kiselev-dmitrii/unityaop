﻿using Assets.Model;
using Assets.UnityAOP.Observable.ChainedObservers;
using UnityEngine;
using Application = Assets.Model.Application;

namespace Assets.Samples.ObservableSample {
public class Sample1 : MonoBehaviour {
    private Application application;
    private ChainedPropertyObserver<int> observer; 

    public void Awake() {
        application = new Application();

    }

    public void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Bind")) {
            observer = application.ObserveProperty(x => x.Player.Group.Members[0].Id, OnValueChanged);
        }
        if (GUI.Button(new Rect(10, 40, 100, 30), "Unbind")) {
            observer.Dispose();
            observer = null;
        }
        if (GUI.Button(new Rect(10, 70, 100, 30), "Change NumMembers")) {
            application.Player.Group.Members.Add(new User(10, "TEst"));
        }
        if (GUI.Button(new Rect(10, 100, 100, 30), "ChangePlayer")) {
            application.Player = new User(2, "New user");
        }

    }

    private void OnValueChanged() {
        Debug.Log(observer.GetValue());
    }
}
}
