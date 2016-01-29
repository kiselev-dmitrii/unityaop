using System.Collections.Generic;

namespace Assets.UnityAOP.Observable.Core {
    public class ObservableImplementation {
        private readonly Dictionary<int, List<IObserver>> propObservers;

        public ObservableImplementation() {
            propObservers = new Dictionary<int, List<IObserver>>();
        }

        public void NotifyPropertyChanged(IObservable self, int memberCode) {
            List<IObserver> observers = null;
            if (propObservers.TryGetValue(memberCode, out observers)) {
                for (var i = 0; i < observers.Count; ++i) {
                    observers[i].OnNodeChanged(self, memberCode);
                } 
            }
        }
    
        public void AddObserver(int memberCode, IObserver observer) {
            List<IObserver> observers = null;
            if (!propObservers.TryGetValue(memberCode, out observers)) {
                observers = new List<IObserver>();
                propObservers[memberCode] = observers;
            }
            observers.Add(observer);
        }
    
        public void RemoveObserver(int memberCode, IObserver observer) {
            List<IObserver> observers = null;
            if (propObservers.TryGetValue(memberCode, out observers)) {
                observers.Remove(observer);
            }
        }
    }
}