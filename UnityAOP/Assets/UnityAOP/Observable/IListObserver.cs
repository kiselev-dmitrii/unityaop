using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Observable {
public interface IListObserver<T> {
    void OnListCleared();
    void OnItemInserted(int index, T item);
    void OnItemRemoved(int index, T item);
}
}
