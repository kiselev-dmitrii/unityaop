using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;

namespace Assets.ObservableTest {
    [Observable]
    public class BaseObservable : IObservable {
        protected ObservableImpl observableImpl;
        protected TypeMetadata metadata;

        private Int32 id;
        public Int32 Id {
            get { return id; }
            private set {
                id = value;
                NotifyPropertyChanged(0);
            }
        }

        private String name;
        public String Name {
            get { return name; }
            private set {
                name = value;
                NotifyPropertyChanged(1);
            }
        }

        public BaseObservable() {
            observableImpl = new ObservableImpl();
            metadata = ObservableMetadata.GetTypeMetadata("BaseObservable");
            observableImpl.SetNumProperties(2);

            Id = 0;
            Name = "Unknown";
        }

        public virtual PropertyMetadata GetPropertyMetadata(string property) {
            return metadata.Properties[property];
        }

        public virtual void AddObserver(int index, IObserver observer) {
            observableImpl.AddObserver(index, observer);
        }

        public virtual void RemoveObserver(int index, IObserver observer) {
            observableImpl.RemoveObserver(index, observer);
        }

        public virtual void NotifyPropertyChanged(int index) {
            observableImpl.NotifyPropertyChanged(this, index);
        }
    }

    public class DerivedObservable : BaseObservable {
        private Int32 numFriends;
        public Int32 NumFriends {
            get { return numFriends; }
            private set {
                numFriends = value;
                NotifyPropertyChanged(4);
            }
        }

        public DerivedObservable() {
            observableImpl.SetNumProperties(3);
            metadata = ObservableMetadata.GetTypeMetadata("DerivedObservable");

            NumFriends = 10;
        }

        public object TestMethod() {
            return null;
        }
    }
}
