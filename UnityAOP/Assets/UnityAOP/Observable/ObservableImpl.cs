using System.Collections.Generic;

namespace Assets.UnityAOP.Observable {
    public class ObservableImpl {
        public List<List<IObserver>> Observers;
    
        public void SetNumProperties(int value) {
            if (Observers == null) {
                Observers = new List<List<IObserver>>();
            }
            if (Observers.Count < value) {
                int diff = value - Observers.Count;
                for (int i = 0; i < diff; ++i) {
                    Observers.Add(new List<IObserver>());
                }
            }
        }
    
        public void NotifyPropertyChanged(IObservable targetObject, int index) {
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
    }
}