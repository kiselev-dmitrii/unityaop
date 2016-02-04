using System.Collections;
using System.Collections.Generic;
using Assets.UnityAOP.Utils;

namespace Assets.UnityAOP.Observable.Core {
    public class ObservableList<T> : IList<T>, IObservable, IObservableList<T>, IObservableList {
        private readonly List<T> list;
        private readonly List<List<IObserver>> indexToObservers;
        private readonly List<IListObserver<T>> typedListObservers;
        private readonly List<IListObserver> untypedListObservers; 
        private readonly bool hasObservableItems;
    
        public ObservableList() {
            list = new List<T>();
            indexToObservers = new List<List<IObserver>>();
            typedListObservers = new List<IListObserver<T>>();
            untypedListObservers = new List<IListObserver>();
            hasObservableItems = typeof (T).HasAttribute<ObservableAttribute>();
        }
        
        #region IList
        public IEnumerator<T> GetEnumerator() {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            int index = list.Count;
            list.Add(item);

            NotifyMemberChanged(index);
            NotifyItemInserted(index, item);
            NotifyItemInserted(index, (object)item);
        }

        public void AddRange(IEnumerable<T> collection) {
            foreach (var item in collection) {
                Add(item);
            }
        }

        public void Clear() {
            list.Clear();

            for (int i = 0; i < indexToObservers.Count; ++i) {
                NotifyMemberChanged(i);
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
                NotifyMemberChanged(i);
            }
            NotifyItemInserted(index, item);
            NotifyItemInserted(index, (object)item);
        }

        public void RemoveAt(int index) {
            var item = list[index];
            list.RemoveAt(index);

            for (int i = index; i < list.Count+1; ++i) {
                NotifyMemberChanged(i);
            }
            NotifyItemRemoved(index, item);
            NotifyItemRemoved(index, (object)item);
        }

        public T this[int index] {
            get { return list[index]; }
            set {
                list[index] = value;
                NotifyMemberChanged(index);
            }
        }
        #endregion

        #region IObservable
        public void AddMemberObserver(int memberCode, IObserver observer) {
            if (memberCode >= indexToObservers.Count) {
                int delta = memberCode - indexToObservers.Count + 1;
                for (int i = 0; i < delta; ++i) {
                    indexToObservers.Add(new List<IObserver>());
                }
            }
            indexToObservers[memberCode].Add(observer);
        }

        public void RemoveMemberObserver(int memberCode, IObserver observer) {
            if (memberCode >= indexToObservers.Count) {
                return;
            }
            indexToObservers[memberCode].Remove(observer);
        }

        public void NotifyMemberChanged(int memberCode) {
            if (memberCode >= indexToObservers.Count) {
                return;
            }
            var observers = indexToObservers[memberCode];
            foreach (var observer in observers) {
                observer.OnNodeChanged(this, memberCode);
            }
        }

        public object GetGetterDelegate(int propertyCode) {
            if (hasObservableItems) {
                return new GetterDelegate<IObservable>(delegate { return (IObservable)TryGet(propertyCode); });
            } else {
                return new GetterDelegate<T>(delegate { return TryGet(propertyCode); });
            }
        }

        public object GetSetterDelegate(int propertyCode) {
            if (hasObservableItems) {
                return new SetterDelegate<IObservable>(delegate(IObservable value) { list[propertyCode] = (T)value; });
            } else {
                return new SetterDelegate<T>(delegate(T value) { list[propertyCode] = value; });
            }
        }

        public object GetMethodDelegate(int methodCode) {
            return null;
        }
        #endregion

        #region IObservableList<T>
        public void AddListObserver(IListObserver<T> observer) {
            typedListObservers.Add(observer);
        }

        public void RemoveListObserver(IListObserver<T> observer) {
            typedListObservers.Remove(observer);
        }

        public void NotifyItemInserted(int index, T item) {
            foreach (var observer in typedListObservers) {
                observer.OnItemInserted(index, item);
            }
        }

        public void NotifyItemRemoved(int index, T item) {
            foreach (var observer in typedListObservers) {
                observer.OnItemRemoved(index, item);
            }
        }

        public object ItemAt(int index) {
            return this[index];
        }

        public void AddListObserver(IListObserver observer) {
            untypedListObservers.Add(observer);
        }

        public void RemoveListObserver(IListObserver observer) {
            untypedListObservers.Remove(observer);
        }

        public void NotifyItemInserted(int index, object item) {
            foreach (var observer in untypedListObservers) {
                observer.OnItemInserted(index, item);
            }
        }

        public void NotifyItemRemoved(int index, object item) {
            foreach (var observer in untypedListObservers) {
                observer.OnItemRemoved(index, item);
            }
        }

        public void NotifyListCleared() {
            foreach (var observer in typedListObservers) {
                observer.OnListCleared();
            }

            foreach (var observer in untypedListObservers) {
                observer.OnListCleared();
            }
        }
        #endregion

        private T TryGet(int index) {
            if (index < list.Count) {
                return list[index];
            } else {
                return default(T);
            }
        }
    }
}
