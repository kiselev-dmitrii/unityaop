using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Observable {
public delegate T GetterDelegate<out T>();
public delegate void SetterDelegate<in T>(T value);

public interface IObservable {
    PropertyMetadata GetPropertyMetadata(String property);
    void AddObserver(int fieldIndex, IObserver observer);
    void RemoveObserver(int fieldIndex, IObserver observer);
    void NotifyPropertyChanged(int fieldIndex);
    object GetGetterDelegate(int propertyIndex);
    object GetSetterDelegate(int propertyIndex);
}
}
