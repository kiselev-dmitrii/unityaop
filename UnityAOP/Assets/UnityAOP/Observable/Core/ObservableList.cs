using System.Collections;
using System.Collections.Generic;
using Assets.UnityAOP.Utils;

namespace Assets.UnityAOP.Observable.Core {
    public class ObservableList<T> : IObservableCollection<T>, IObservable,  IList<T> {
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
        }

        public void RemoveAt(int index) {
            var item = list[index];
            list.RemoveAt(index);

            for (int i = index; i < list.Count+1; ++i) {
                NotifyMemberChanged(i);
            }
            NotifyItemRemoved(index, item);
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

        #region IObservableCollection
        public void AddCollectionObserver(IListObserver<T> observer) {
            listObservers.Add(observer);
        }

        public void RemoveCollectionObserver(IListObserver<T> observer) {
            listObservers.Remove(observer);
        }

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
    
        private T TryGet(int index) {
            if (index < list.Count) {
                return list[index];
            } else {
                return default(T);
            }
        }
        #endregion


    }
}
