using System;
using System.Collections;
using System.Collections.Generic;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;

namespace Assets.UnityAOP.Observable {
public class ObservableList<T> : IObservableCollection, IObservable,  IList<T> {
    private readonly List<T> list;
    private readonly List<List<IObserver>> indexToObservers;
    private readonly List<IListObserver<T>> listObservers; 
    private readonly bool hasObservableItems;

    public ObservableList() {
        list = new List<T>();
        indexToObservers = new List<List<IObserver>>();
        listObservers = new List<IListObserver<T>>();
        hasObservableItems = typeof (T).HasAttribute<ObservableAttribute>();
    } 

    public PropertyMetadata GetPropertyMetadata(string property) {
        throw new NotImplementedException();
        //NOTUSED
    }

    public void AddObserver(int fieldIndex, IObserver observer) {
        if (fieldIndex >= indexToObservers.Count) {
            int delta = fieldIndex - indexToObservers.Count + 1;
            for (int i = 0; i < delta; ++i) {
                indexToObservers.Add(new List<IObserver>());
            }
        }
        indexToObservers[fieldIndex].Add(observer);
    }

    public void RemoveObserver(int fieldIndex, IObserver observer) {
        if (fieldIndex >= indexToObservers.Count) {
            return;
        }
        indexToObservers[fieldIndex].Remove(observer);
    }

    public void NotifyPropertyChanged(int fieldIndex) {
        if (fieldIndex >= indexToObservers.Count) {
            return;
        }
        var observers = indexToObservers[fieldIndex];
        foreach (var observer in observers) {
            observer.OnNodeChanged(this, fieldIndex);
        }
    }

    public object GetGetterDelegate(int propertyIndex) {
        if (hasObservableItems) {
            return new GetterDelegate<IObservable>(delegate { return (IObservable) TryGet(propertyIndex); });
        } else {
            return new GetterDelegate<T>(delegate { return TryGet(propertyIndex); });
        }
    }

    public object GetSetterDelegate(int propertyIndex) {
        if (hasObservableItems) {
            return new SetterDelegate<IObservable>(delegate(IObservable value) { list[propertyIndex] = (T)value; });
        } else {
            return new SetterDelegate<T>(delegate(T value) { list[propertyIndex] = value; });
        }
    }

    public IEnumerator<T> GetEnumerator() {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(T item) {
        int index = list.Count;
        list.Add(item);

        NotifyPropertyChanged(index);
        NotifyItemInserted(index, item);
    }

    public void Clear() {
        list.Clear();

        for (int i = 0; i < indexToObservers.Count; ++i) {
            NotifyPropertyChanged(i);
        }
        NotifyListCleared();
    }

    public bool Contains(T item) {
        return list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    public int Count {
        get { return list.Count; }
    }

    public bool IsReadOnly {
        get { return ((IList<T>) list).IsReadOnly; }
    }

    public int IndexOf(T item) {
        return list.IndexOf(item);
    }

    public void Insert(int index, T item) {
        list.Insert(index, item);

        for (int i = index; i < list.Count; ++i) {
            NotifyPropertyChanged(i);
        }
        NotifyItemInserted(index, item);
    }

    public void RemoveAt(int index) {
        var item = list[index];
        list.RemoveAt(index);

        for (int i = index; i < list.Count+1; ++i) {
            NotifyPropertyChanged(i);
        }
        NotifyItemInserted(index, item);
    }

    public T this[int index] {
        get { return list[index]; }
        set {
            list[index] = value;
            NotifyPropertyChanged(index);
        }
    }

    public T TryGet(int index) {
        if (index < list.Count) {
            return list[index];
        } else {
            return default(T);
        }
    }

    #region Notify methods
    public void NotifyItemInserted(int index, T item) {
        foreach (var observer in listObservers) {
            observer.OnItemInserted(index, item);
        }
    }

    public void NotifyItemRemoved(int index, T item) {
        foreach (var observer in listObservers) {
            observer.OnItemRemoved(index, item);
        }
    }

    public void NotifyListCleared() {
        foreach (var observer in listObservers) {
            observer.OnListCleared();
        }
    }
    #endregion
}
}
