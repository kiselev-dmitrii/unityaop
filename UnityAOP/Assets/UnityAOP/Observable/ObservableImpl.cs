using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.UnityAOP.Observable {
public class ObservableImpl<T> {
    public static Dictionary<String, int> PropertyIndices;
    public List<IObserver>[] Observers;

    public ObservableImpl(String[] observableProperties) {
        if (PropertyIndices == null) {
            Debug.Log("Initializing property indices for " + typeof(T).Name);
            PropertyIndices = new Dictionary<string, int>();
            for (int i = 0; i < observableProperties.Length; ++i) {
                PropertyIndices.Add(observableProperties[i], i);
            }
        }

        Observers = new List<IObserver>[observableProperties.Length];
        for (int i = 0; i < observableProperties.Length; i++) {
            Observers[i] = new List<IObserver>();
        }
    } 

    public void NotifyPropertyChanged(object targetObject, int index) {
        var observers = Observers[index];
        for (var i = 0; i < observers.Count; ++i) {
            observers[i].OnNodeChanged(targetObject, index);
        }
    }

    public void AddObserver(int index, IObserver observer) {
        Observers[index].Add(observer);
    }

    public void RemoveObserver(int index, IObserver observer) {
        Observers[index].Remove(observer);
    }

    public int GetIndexOfProperty(string property) {
        return PropertyIndices[property];
    }
}
}